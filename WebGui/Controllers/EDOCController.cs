using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using EDOCApi;
using Prolink.DataOperation;
using Prolink.Data;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using System.Threading;
using Business;
using System.ComponentModel;

namespace WebGui.Controllers
{
    public class EDOCController : BaseController
    {
        //
        // GET: /EDOC/
        private Prolink.EDOC_API _api = new Prolink.EDOC_API();
        private string _folderID = "";
        private static int m_Timeout = 300000;
        private static string cloumns = "FOLDER_GUID,SERVER_NUM";

        public Prolink.EDOC_API GetEdocControllerEDOC_API()
        {
            if (_api == null)
                _api = new Prolink.EDOC_API();
            return _api;
        }

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 檔案上傳
        /// </summary>
        /// <returns></returns>
        public ActionResult FileUpload()
        {
            //_api.Login();
            List<EDOCFileItem> edocList = _api.Inquery("b4b4aad1-f72f-4618-9562-cb0b8967d9ff");
            ViewBag.edocList = ToContent(edocList);


            return View();
        }
        [HttpPost]
        public ActionResult FileDownload()
        {

            string filePath = Request.Params["filePath"];
            string itemID = Request.Params["itemID"];

            //_api.Login();
            _api.DownloadFile(filePath, itemID, "", getServerNum(itemID));
            return ToContent(itemID);


        }
        [HttpPost]
        public ActionResult GetEdocServer()
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = null;

            item = new Dictionary<string, object>();
            string itemID = Request.Params["itemID"];
            int serverNum = getServerNum(itemID);
            string path = WebConfigurationManager.AppSettings["EDOC_URL"];
            if (serverNum > 0)
                path = WebConfigurationManager.AppSettings["EDOC_URL_" + serverNum];
            item["url"] = path;
            item["softID"] = WebConfigurationManager.AppSettings["EDOC_SOFTID"];
            item["account"] = WebConfigurationManager.AppSettings["EDOC_ACCOUNT"];
            item["password"] = WebConfigurationManager.AppSettings["EDOC_PASSWORD"];
            list.Add(item);

            return ToContent(list);

        }


        #region 公告文件查询，上传
        [HttpPost]
        public ActionResult AttachFileQuery()
        {
            string jobNo = Request.Params["JOBNO"];
            string groupId = Request.Params["GROUP_ID"];
            string cmp = Request.Params["CMP"];
            string stn = Request.Params["STN"];
            string dep = "";// Request.Params["DEP"];
            string type = String.IsNullOrEmpty(Request.Params["TYPE"]) ? "" : Request.Params["TYPE"];
            bool FileDel = Prolink.Math.GetValueAsBool(Request.Params["FILEDEL"]);//true:公告栏建档界面,false:公告栏界面

            if (type == null || type == "")
            {
                string edtPri = GetBtnPms("EDOC") + "|";
                type = edtPri.Replace("EDOC_EDT_V_", "").Replace("|", ";");
            }

            if (type.Equals(";"))
            {
                type = "null";
            }

            int serverNum = 0;
            string guid = _api.GetFolderGUID(jobNo, groupId, cmp, stn, dep, ref serverNum, cloumns);
            List<EDOCFileItem> edocList = _api.Inquery(guid, type, serverNum);
            SortEdocList(edocList);

            #region 更新公告栏是否有文件Flag
            string sql = string.Format("SELECT ATTACH_FLAG FROM MOD_BULLETIN WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0 && FileDel)
            {
                string attachFlag = Prolink.Math.GetValueAsString(dt.Rows[0]["ATTACH_FLAG"]);
                if (edocList != null && edocList.Count > 0 && "Y" != attachFlag)
                {
                    EditInstruct ei = new EditInstruct("MOD_BULLETIN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", jobNo);
                    ei.Put("ATTACH_FLAG", "Y");
                    OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                else if (edocList != null && edocList.Count == 0 && "Y" == attachFlag)
                {
                    EditInstruct ei = new EditInstruct("MOD_BULLETIN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", jobNo);
                    ei.Put("ATTACH_FLAG", "N");
                    OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            #endregion

            return ToContent(edocList);
        }

        [HttpPost]
        public ActionResult AttachFileUpload(HttpPostedFileBase data)
        {
            var files = Request.Files;
            string jobNo = Request.Params["jobNo"];
            string groupId = Request.Params["GROUP_ID"];
            string cmp = Request.Params["CMP"];
            string stn = Request.Params["STN"];
            string dep = "";
            string type = Request.Params["TYPE"] != null ? Request.Params["TYPE"] : "";
            string remark = Request.Params["REMARK"] != null ? Request.Params["REMARK"] : "";

            EDOCFileItem fileInfo = null;
            int fileCount = 0;
            foreach (string uploadId in files)
            {
                HttpPostedFileBase file = files[fileCount++];

                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    if (!GetFileExtensions(ref fileName))
                    {
                        return Json(new { message = "This file type is not support to upload!" });
                    }
                    var path = Path.Combine(Server.MapPath("~/FileUploads"), fileName);
                    file.SaveAs(path);
                    int serverNum = 0;
                    fileInfo = AttachUploadFile2EDOC(jobNo, path, groupId, cmp, stn, dep, ref serverNum, remark);
                    if (fileInfo == null)
                    {
                        return Json(new { message = "Upload to Edoc error,Folder GuID is null,Please check EDI LOG!" });
                    }
                    if (!string.IsNullOrEmpty(type))
                    {
                        List<UpdateFileItem> fileList = new List<UpdateFileItem>();
                        fileList.Add(new UpdateFileItem
                        {
                            FileID = fileInfo.FileID,
                            EdocType = type,
                            Remark = remark
                        });
                        bool isSuccess = _api.UpdateFiles(fileList, serverNum);
                    }
                    try
                    {
                        System.IO.File.Delete(path);
                    }
                    catch (Exception ex) { }
                }
            }
            return ToContent(fileInfo);
        }

        private EDOCFileItem AttachUploadFile2EDOC(string jobNo, string filePath, string groupId, string cmp, string stn, string dep, ref int serverNum, string remark = "")
        {
            EDOCFileItem data = null;
            EDOCResultUploadFile uploadResult = null;

            try
            {
                CheckDuplicatedFolder(jobNo, groupId, cmp, stn, "", ref serverNum);
                if (string.IsNullOrEmpty(_folderID))
                {
                    return data;
                }
                if (string.IsNullOrEmpty(remark))
                {
                    remark = "Remark";
                }
                uploadResult = _api.UploadFile(_folderID, filePath, remark, UserId, "D", "8", serverNum);
                if (uploadResult != null && uploadResult.Status == DBErrors.DB_SUCCESS)
                {
                    data = uploadResult.FileInfo;
                }
            }
            catch (Exception ex)
            {

            }

            return data;
        }

        private static void SortEdocList(List<EDOCFileItem> edocList)
        {
            if (edocList == null)
                return;
            edocList.Sort(
               delegate (EDOCFileItem p1, EDOCFileItem p2)
               {
                   DateTime? sendTime1 = null;
                   DateTime? sendTime2 = null;

                   DateTime t1 = DateTime.Now;
                   if (!string.IsNullOrEmpty(p1.SendTime) && DateTime.TryParse(p1.SendTime.Replace("T", " "), out t1))
                   {
                       sendTime1 = t1;
                   }

                   DateTime t2 = DateTime.Now;
                   if (!string.IsNullOrEmpty(p1.SendTime) && DateTime.TryParse(p2.SendTime.Replace("T", " "), out t2))
                   {
                       sendTime2 = t2;
                   }
                   if (sendTime1 == null && sendTime2 == null)
                       return 0;
                   else if (sendTime1 == null && sendTime2 != null)
                       return 1;
                   else if (sendTime1 != null && sendTime2 == null)
                       return -1;
                   else
                       return sendTime2.Value.CompareTo(sendTime1.Value);
               });
        }

        #endregion


        [HttpPost]
        public ActionResult FileQuery()
        {

            string jobNo = Request.Params["JOBNO"];
            string groupId = Request.Params["GROUP_ID"];
            string cmp = Request.Params["CMP"];
            string stn = Request.Params["STN"];
            string dep = "";// Request.Params["DEP"];
            string type = String.IsNullOrEmpty(Request.Params["TYPE"]) ? "":Request.Params["TYPE"] ;

            string BaseType = String.IsNullOrEmpty(Request.Params["BaseType"]) ? "" : Request.Params["BaseType"];

            if (type == null || type == "")
            {
                string edtPri = GetBtnPms("EDOC") + "|";

                if ("Insu".Equals(BaseType))
                {
                    string _type = edtPri.Replace("EDOC_EDT_V_", "*");
                    string[] types = _type.Split('*');
                    for (int i = 1; i < types.Length; i++)
                    {
                        if (i == 1)
                        {
                            type = types[0];
                        }

                        string[] m = types[i].Split('|');
                        for (int k = 0; k < m.Length; k++)
                        {
                            if ("PACKI;INVI;BL_confirm;".Contains(m[k]))
                            {
                                type += m[k]+"|";
                            }
                        }
                       
                    }
                    type = type.Replace("|", ";");
                }
                else
                {
                    type = edtPri.Replace("EDOC_EDT_V_", "").Replace("|", ";");
                }
            }
            if (type.Equals(";"))
            {
                type = "null";
            }
            //List<Dictionary<string, string>> list = FileQueryDownlodInfo(jobNo, groupId, cmp, stn, dep, type);
            //_api.Login();
            int serverNum = 0;
            string guid = _api.GetFolderGUID(jobNo, groupId, cmp, stn, dep, ref serverNum, cloumns);
            List<EDOCFileItem> edocList = _api.Inquery(guid, type, serverNum);

            if (IsGuidByParse(jobNo) && (edocList != null && edocList.Count > 0))
            {
                string ty = "Y";
                UpdateDnMsg(jobNo, groupId, cmp, ty);
                for (int i = 0; i < edocList.Count(); i++)
                {
                    EDOCFileItem edocei = edocList[i];
                    if (string.IsNullOrEmpty(edocei.EdocType) && !UserId.Equals(edocei.Uploader))
                    {
                        edocList.Remove(edocei);
                        i--;
                    }
                }
            }
            return ToContent(edocList);
        }
        public ActionResult FileCheck()
        {
            string jobNo =Prolink.Math.GetValueAsString( Request.Params["JOBNO"]);
            string groupId = Prolink.Math.GetValueAsString(Request.Params["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(Request.Params["CMP"]);
            string type = !String.IsNullOrEmpty(Request.Params["TYPE"]) ? Request.Params["TYPE"] : "";
            string smdoc = Prolink.Math.GetValueAsString( Request.Params["smdoc"]);
            bool result = CheckEdoc(jobNo, groupId, cmp, type, smdoc);
            if (result)
            {
                string msg = string.Empty;
                switch (type)
                {
                    case "INVO": msg = @Resources.Locale.L_Edoc_BeHaveI; break;
                    case "PACKO": msg = @Resources.Locale.L_Edoc_BeHaveP; break;
                    case "BF": msg = @Resources.Locale.L_Edoc_BeHaveBF; break;
                    case "BFC": msg = @Resources.Locale.L_Edoc_BeHaveBFC; break;
                }
                return Json(new { message = msg, IsOk = "N" });
            }
            else
            {
                return Json(new { message = "", IsOk = "Y" });
            }
        }
        public bool CheckEdoc(string jobNo, string groupID, string cmp, string type, string smdoc)
        {
            type = type == "BFC" ? "BF" : type;
            if (!string.IsNullOrEmpty(smdoc) && smdoc.Equals("Y"))
            {
                return GetEdoc(jobNo, groupID, cmp, type);
            }
            else
            {
                string sql = string.Format("select * from sminm where U_ID={0}", SQLUtils.QuotedStr(jobNo));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string dn = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                        if (string.IsNullOrEmpty(dn))
                        {
                            string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                            string sql1 = string.Format("SELECT U_ID FROM SMSM WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(shipmentid));
                            string uid = OperationUtils.GetValueAsString(sql1, Prolink.Web.WebContext.GetInstance().GetConnection());
                            return GetEdoc(uid, groupID, cmp, type);
                        }
                        else
                        {

                            string sql1 = string.Format("SELECT U_ID FROM SMDN WHERE DN_NO ={0}", SQLUtils.QuotedStr(dn));
                            string uid = OperationUtils.GetValueAsString(sql1, Prolink.Web.WebContext.GetInstance().GetConnection());
                            return GetEdoc(uid, groupID, cmp, type);
                        }
                    }
                }
            }
            return false;
        }
        public bool GetEdoc(string jobNo, string groupID, string cmp, string type)
        {
            string condition = " GROUP_ID=" + SQLUtils.QuotedStr(groupID)
                             + " AND CMP=" + SQLUtils.QuotedStr(cmp)
                             + " AND JOB_NO=" + SQLUtils.QuotedStr(jobNo);

            string sql = string.Format("SELECT COUNT(*) FROM FILES WHERE FID IN (SELECT FID FROM FOLDERS WHERE GUID IN (SELECT FOLDER_GUID FROM EDOC2_FOLDER WHERE {0})) AND EdocType={1}", condition, SQLUtils.QuotedStr(type));
            int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return count > 0 ? true : false;
        }
        static bool IsGuidByParse(string strSrc)
        {
            Guid g = Guid.Empty;
            //return Guid.TryParse(strSrc, out g);
            return Guid.TryParseExact(strSrc, "D", out g);
        }
        
        [HttpPost]
        public ActionResult FileDelete()
        {

            string itemID = Request.Params["itemID"];
            string jobNo = Request.Params["jobNo"];
            if (!string.IsNullOrEmpty(jobNo))
            {
                InsertTMEXP(itemID, jobNo);
            }
            //_api.Login();
            _api.DeleteFile(itemID, getServerNum(itemID));
            return ToContent(itemID);


        }

        public ActionResult FileDeleteback()
        {
            string jobNo = Prolink.Math.GetValueAsString(Request.Params["jobNo"]);
            string groupId = Prolink.Math.GetValueAsString(Request.Params["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(Request.Params["CMP"]);
            string stn = Request.Params["STN"];
            string dep = "";
            string type = String.IsNullOrEmpty(Request.Params["TYPE"]) ? Request.Params["TYPE"] : "";

            if (type == null || type == "")
            {
                string edtPri = GetBtnPms("EDOC") + "|";

                type = edtPri.Replace("EDOC_EDT_V_", "").Replace("|", ";");
            }
            string guid = string.Empty;
            int count = 0;
            int serverNum = 0;
            guid = _api.GetFolderGUID(jobNo, groupId, cmp, stn, dep, ref serverNum, cloumns);
            List<EDOCFileItem> edocList = _api.Inquery(guid, type, serverNum);
            count = edocList.Count;
            string sql = string.Format("SELECT * FROM SMDN WHERE U_ID='{0}'", jobNo);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (count <= 0)
            {
                if (dt.Rows.Count > 0)
                {


                    string shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                    sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID='{0}'", shipmentid);
                    DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (smdt.Rows.Count > 0)
                    {
                        jobNo = Prolink.Math.GetValueAsString(smdt.Rows[0]["U_ID"]);
                    }
                    serverNum = 0;
                    guid = _api.GetFolderGUID(jobNo, groupId, cmp, stn, dep, ref serverNum, cloumns);
                    edocList = _api.Inquery(guid, type, serverNum);
                    count += edocList.Count;
                }
                else
                {
                    sql = string.Format("SELECT * FROM SMSM WHERE U_ID='{0}'", jobNo);
                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count > 0 && count <= 0)
                    {
                        string shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                        sql = string.Format("SELECT * FROM SMDN WHERE SHIPMENT_ID='{0}'", shipmentid);
                        DataTable ndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (ndt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in ndt.Rows)
                            {
                                string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                                serverNum = 0;
                                guid = _api.GetFolderGUID(jobNo, groupId, cmp, stn, dep, ref serverNum, cloumns);
                                edocList = _api.Inquery(guid, type, serverNum);
                                count += edocList.Count;
                            }
                        }
                    }
                }
                if (count <= 0)
                    UpdateDnMsg(jobNo, groupId, cmp, "");
            }
            return ToContent("");
        }

        [HttpPost]
        public ActionResult FileUpload(HttpPostedFileBase data)
        {
            var files = Request.Files;
            string jobNo = Request.Params["jobNo"];
            string groupId = Request.Params["GROUP_ID"];
            string cmp = Request.Params["CMP"];
            string type = Request.Params["TYPE"] != null ? Request.Params["TYPE"] : "";
            string remark = Request.Params["REMARK"] != null ? Request.Params["REMARK"] : "";
            string reserve_no = Request.Params["RESERVE_NO"];
            string wh = Request.Params["WH"];
            string call_type = Request.Params["CALL_TYPE"];
            EDOCFileItem fileInfo = null;
            int fileCount = 0;

            Dictionary<int, string> dic = new Dictionary<int, string>();
            foreach (string uploadId in files)
            {
                HttpPostedFileBase file = files[fileCount++];

                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    if (!GetFileExtensions(ref fileName))
                    {
                        return Json(new { message = "This file type is not support to upload!" });
                    }
                    var path = Path.Combine(Server.MapPath("~/FileUploads"), fileName);
                    file.SaveAs(path);
                    int serverNum = 0;
                    bool isSuccess = false;
                    UploadFiles(_api,UserId,jobNo, groupId, cmp, type, remark,ref fileInfo, dic, path, ref serverNum, ref isSuccess);

                    if ("POD".Equals(type) && isSuccess && !string.IsNullOrEmpty(reserve_no) && !string.IsNullOrEmpty(wh))
                    {
                        BatchUploadPOD(_api, UserId, jobNo, groupId, cmp, type, remark, reserve_no, wh, call_type, fileInfo, dic, path, ref serverNum, ref isSuccess);
                    }
                }
            }

            return ToContent(fileInfo);


        }

        public static void BatchUploadPOD(Prolink.EDOC_API _api, string userid, string jobNo, string groupId, string cmp, string type, string remark, string reserve_no, string wh, string call_type, EDOCFileItem fileInfo, Dictionary<int, string> dic, string path, ref int serverNum, ref bool isSuccess)
        {
            string sql = string.Format("SELECT SHIPMENT_ID,SHIPMENT_INFO,IBAT_NO FROM SMIRV WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                PODUploadHandle.UploadPODHandle(jobNo, reserve_no, wh, call_type, cmp);
                string shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                if (string.IsNullOrEmpty(shipmentid))
                {
                    shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_INFO"]);
                    string[] shipments = shipmentid.Split(',');
                    if (shipments.Length > 0)
                        shipmentid = shipments[0];
                }
                sql = string.Format("select tran_type from smsmi where smsmi.shipment_id ={0}", SQLUtils.QuotedStr(shipmentid));
                string trantype = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if ("T".Equals(trantype))
                {
                    string ibatno = Prolink.Math.GetValueAsString(dt.Rows[0]["IBAT_NO"]);
                    if (!string.IsNullOrEmpty(ibatno))
                    {
                        string smirvsql = string.Format("SELECT U_ID,RESERVE_NO,CALL_TYPE FROM SMIRV WHERE IBAT_NO={0} AND RESERVE_NO!={1}",
                       SQLUtils.QuotedStr(ibatno), SQLUtils.QuotedStr(reserve_no));
                        DataTable smirvdt = OperationUtils.GetDataTable(smirvsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        foreach (DataRow dr in smirvdt.Rows)
                        {
                            string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                            BatchUplaodFiles(_api,userid, uid, groupId, cmp, type, remark,ref fileInfo, dic, path, ref serverNum, ref isSuccess);
                            string reserveno = Prolink.Math.GetValueAsString(dr["RESERVE_NO"]);
                            string calltype = Prolink.Math.GetValueAsString(dr["CALL_TYPE"]);
                            PODUploadHandle.UploadPODHandle(uid, reserveno, wh, calltype, cmp);
                        }
                    }
                }
            }
        }

        public static void UploadFiles(Prolink.EDOC_API _api, string userid, string jobNo, string groupId, string cmp, string type, string remark, ref EDOCFileItem fileInfo, Dictionary<int, string> dic, string path, ref int serverNum, ref bool isSuccess)
        {
            string folderID = Business.TPV.Utils.EDocHelper.CheckDuplicatedFolder(_api, jobNo, groupId, cmp, "*", "", ref serverNum);
            EDOCResultUploadFile uploadResult = _api.UploadFile(folderID, path, "", userid, "D", "8", serverNum);
            if (fileInfo == null)
            {
                fileInfo = uploadResult.FileInfo;
            }
            List<UpdateFileItem> fileList = new List<UpdateFileItem>();
            fileList.Add(new UpdateFileItem
            {
                FileID = fileInfo.FileID,
                EdocType = type,
                Remark = remark
            });
            isSuccess = _api.UpdateFiles(fileList, serverNum);
        }

        public static void BatchUplaodFiles(Prolink.EDOC_API _api,string userid,string jobNo, string groupId, string cmp, string type, string remark,ref EDOCFileItem fileInfo, Dictionary<int, string> dic, string path, ref int serverNum, ref bool isSuccess)
        {
            string folderID = Business.TPV.Utils.EDocHelper.CheckDuplicatedFolder(_api, jobNo, groupId, cmp, "*", "", ref serverNum);
            if (!dic.ContainsKey(serverNum))
            {
                EDOCResultUploadFile uploadResult = _api.UploadFile(folderID, path, "", userid, "D", "8", serverNum);
                if (fileInfo == null)
                {
                    fileInfo = uploadResult.FileInfo;
                }
                List<UpdateFileItem> fileList = new List<UpdateFileItem>();
                fileList.Add(new UpdateFileItem
                {
                    FileID = fileInfo.FileID,
                    EdocType = type,
                    Remark = remark
                });
                isSuccess = _api.UpdateFiles(fileList, serverNum);
                dic.Add(serverNum, fileInfo.FileID);
            }
            else
                _api.CopyFile(dic[serverNum], folderID, serverNum);
        }

        
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SendMail()
        {
            string mailTo = Prolink.Math.GetValueAsString(Request.Params["mailTo"]);
            string mailContent = Prolink.Math.GetValueAsString(Request.Params["mailContent"]).Replace("{EDOC_URL}", WebConfigurationManager.AppSettings["EDOC_URL1"]);

            string sql = "SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID=" + SQLUtils.QuotedStr(UserId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            string userEmail = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            mailTo = mailTo + ";" + userEmail;
            EvenFactory.AddEven(UserId + "#" + GroupId + "#" + CompanyId + "#" + Guid.NewGuid().ToString(), UserId, MailManager.EdocList, null, 1, 0, mailTo, "EDOC", mailContent);
            return ToContent("");
        }


        [HttpPost]
        public ActionResult UpdateEdoc()
        {

            //EDOCAgent.Agent.UploadProgress += new FileProgressEventHandler(Agent_UploadProgress);
            string[] files = Request.Form.ToString().Split('&');
            foreach (string file in files)
            {
                List<UpdateFileItem> fileList = new List<UpdateFileItem>();
                string changefile = file.Replace("+", "%20");
                changefile = Server.UrlDecode(changefile);
                string[] fileAtt = changefile.Split('=');
                string fileId = fileAtt[0].Replace("_Edt", "");
                fileList.Add(new UpdateFileItem
                {
                    FileID = fileId,
                    EdocType = fileAtt[1]
                    //Remark = ""
                });
                _api.UpdateFiles(fileList, getServerNum(fileId));

            }

            //_api.Login();

            //bool isSuccess = _api.UpdateFiles(fileList);
            return ToContent("");
        }

        private EDOCFileItem UploadFile2EDOC(string jobNo, string filePath, string groupId, string cmp, string stn, string dep, string remark, ref int serverNum)
        {
            EDOCFileItem data = null;
            EDOCResultUploadFile uploadResult = null;
            try
            {
                //_api.Login();
                CheckDuplicatedFolder(jobNo, groupId, cmp, stn, "", ref serverNum);
                if (string.IsNullOrEmpty(_folderID))
                {
                    return data;
                }
                uploadResult = _api.UploadFile(_folderID, filePath, "Remark", UserId, "D", "8", serverNum);
                //uploadResult = _api.UploadFile(jobNo, filePath, "", userId, "D", "8");
                if (uploadResult != null && uploadResult.Status == DBErrors.DB_SUCCESS)
                {
                    //InitGrid();
                    data = uploadResult.FileInfo;
                    if (IsGuidByParse(jobNo))
                        UpdateDnMsg(jobNo, groupId, cmp, "Y");
                }
            }
            catch (Exception ex)
            {

            }

            return data;

        }

        public void UpdateDnMsg(string jobNo, string groupId, string cmp, string flag)
        {
            string sql = string.Format(@"SELECT *
                  FROM SMDN WHERE U_ID = '{0}' AND GROUP_ID = '{1}'
                   AND CMP = '{2}'", jobNo, groupId, cmp);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                Updatedn(jobNo, groupId, cmp, flag);
            }
            else {
                sql = string.Format(@"SELECT *
                  FROM SMSM WHERE U_ID = '{0}' AND GROUP_ID = '{1}'
                   AND CMP = '{2}'", jobNo, groupId, cmp);
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count > 0)
                {
                    string dnno = Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]);
                    sql = string.Format(@"UPDATE SMDN SET EDOC='{0}' WHERE DN_NO = '{1}'",flag, dnno);
                    OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
        }
        public void Updatedn(string jobNo, string groupId, string cmp, string flag)
        {
            string sql = string.Format(@"UPDATE SMDN SET EDOC='{0}' WHERE U_ID = '{1}' AND GROUP_ID = '{2}'
                   AND CMP = '{3}'",flag, jobNo, groupId, cmp);
            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
        public static EDOCFileItem UploadFile2EDOC(string jobNo, string filePath, string groupId, string cmp, string stn, string userid, string fileType = "RFQ", string remark = "", string dep = "")
        {
            Prolink.EDOC_API _api = new Prolink.EDOC_API();
            EDOCFileItem data = null;
            EDOCResultUploadFile uploadResult = null;
            try
            {
                int serverNum = 0;
                //_api.Login();
                string folderID = Business.TPV.Utils.EDocHelper.CheckDuplicatedFolder(_api, jobNo, groupId, cmp, stn, "",ref serverNum);
                uploadResult = _api.UploadFile(folderID, filePath, remark, userid, "D", "8",serverNum);
                //uploadResult = _api.UploadFile(jobNo, filePath, "", userId, "D", "8");
                OperationUtils.Logger.WriteLog("UploadFile2EDOC=" + jobNo + groupId + cmp + stn );
                if (uploadResult != null && uploadResult.Status == DBErrors.DB_SUCCESS)
                {
                    //InitGrid();
                    data = uploadResult.FileInfo;
                    //fileType
                    List<UpdateFileItem> fileList = new List<UpdateFileItem>();
                    fileList.Add(new UpdateFileItem
                    {
                        FileID = data.FileID,
                        EdocType = fileType,
                        Remark = remark
                    });

                    bool isSuccess = _api.UpdateFiles(fileList, serverNum);
                }
            }
            catch (Exception ex)
            {
                OperationUtils.Logger.WriteLog("UploadFile2EDOC=" + jobNo + groupId  + cmp + stn + ex.ToString() );
            }
            return data;
        }



        public int getServerNum(string FileID)
        {
            string sql = string.Format("SELECT SERVER_NUM FROM EDOC2_FOLDER WHERE FOLDER_GUID= (SELECT TOP 1 GUID FROM Folders WHERE FID=(SELECT TOP 1 FID FROM Files WHERE FileID={0}))", SQLUtils.QuotedStr(FileID));
            return OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        private void CheckDuplicatedFolder(string job_no, string groupID, string cmp, string stn, string dep, ref int serverNum)
        {
            try
            {
                _folderID = _api.GetFolderGUID(job_no, groupID, cmp, stn, dep, ref serverNum, cloumns);
                if (string.IsNullOrEmpty(_folderID))
                {
                    _folderID = _api.SetNewFolder(ref serverNum); //沒查到才向EDOC索取並寫進table
                    if (!string.IsNullOrEmpty(_folderID))
                        _api.SetFolderIDInDB(job_no, _folderID, groupID, cmp, stn, dep, true, serverNum);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (message.Length > 500) message = message.Substring(0, 500);
                EditInstruct ei = new EditInstruct("EDI_LOG", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.Put("EDI_ID", "GetFolderGuid");
                ei.PutExpress("EVENT_DATE", "getdate()");
                ei.Put("REMARK", message);
                ei.Put("SENDER", "Edoc Server");
                ei.Put("RS", "Receive");
                ei.Put("STATUS", "Exception");
                ei.Put("FROM_CD", "Edoc Server");
                ei.Put("TO_CD", "Eshipping");
                ei.Put("DATA_FOLDER", "");
                ei.Put("REF_NO", job_no);
                ei.Put("GROUP_ID", "TPV");
                ei.Put("CMP", cmp);
                ei.Put("STN", stn);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            _folderID = "";
        }

        public ActionResult CreateNewReport2Edoc()
        {
            Result result = new Result();
            Dictionary<string, object> item = null;
            item = new Dictionary<string, object>();
            item["url"] = WebConfigurationManager.AppSettings["EDOC_URL1"]; // get outer url for edoc 
            item["softID"] = WebConfigurationManager.AppSettings["EDOC_SOFTID"];
            item["account"] = WebConfigurationManager.AppSettings["EDOC_ACCOUNT"];
            item["password"] = WebConfigurationManager.AppSettings["EDOC_PASSWORD"];
            item["syncTime"] = Prolink.Math.GetValueAsString(WebConfigurationManager.AppSettings["EDOC_SYNC_TIME"]);
            item["syncDnCount"] = Prolink.Math.GetValueAsString(WebConfigurationManager.AppSettings["EDOC_SYNC_DN_COUNT"]);
            if (item["syncTime"] == "")
            {
                item["syncTime"] = "180000";//3 min
            }
            if (item["syncDnCount"] == "")
            {
                item["syncDnCount"] = "10";//10 count
            }

            int combineCount = 0;

            string reportId = Request.Params["reportId"];
            string conditionString = Request.Params["conditionString"];
            string exportFileType = Request.Params["exportFileType"];
            string reportName = Request.Params["reportName"];
            string jobNo = Request.Params["jobNo"];
            string groupId = Request.Params["GroupId"];
            string cmp = Request.Params["CMP"];
            string stn = Request.Params["STN"];
            string dep = "";// Request.Params["DEP"];
            string fileType = Request.Params["fileType"];
            string remark = "";
            string shipmentid = Request.Params["ShipmentId"];
            string sql = string.Empty;

            if (!string.IsNullOrEmpty(Request.Params["remark"]))
            {
                remark = Request.Params["remark"];
            }
            if (!string.IsNullOrEmpty(Request.Params["combineInfo"]))
            {
                combineCount = Prolink.Math.GetValueAsString(Request.Params["combineInfo"]).Split(',').Length;
            }

          
            if (!string.IsNullOrEmpty(shipmentid))
            {
                sql = string.Format("SELECT U_ID,GROUP_ID,CMP,COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid).Trim());
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count > 0)
                {
                    DataRow dr=dt.Rows[0];
                    jobNo = Prolink.Math.GetValueAsString(dr["U_ID"]);
                    groupId = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                    cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                    combineCount = Prolink.Math.GetValueAsString(dr["COMBINE_INFO"]).Split(',').Length;
                }
            }

            string baseUrl = WebConfigurationManager.AppSettings["LOCAL_URL"];

            int serverNum = 0;
            CheckDuplicatedFolder(jobNo, groupId, cmp, stn, "", ref serverNum);
            string edocFGUID = _folderID;
            if (string.IsNullOrEmpty(edocFGUID))
            {
                result.Success = false;
                result.Message = "Upload to Edoc error,Folder GuID is null,Please check EDI LOG!";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            string tempExportFile = null;
            string reportTitle = System.DateTime.Now.ToString("yyyy-MM-dd HH mm ss ") + reportName;
            try
            {
                NameValueCollection formparams = new NameValueCollection();
                formparams.Add("{0}", reportName);
                formparams.Add("{1}", reportId);
                formparams.Add("{2}", exportFileType);
                formparams.Add("{3}", System.Web.HttpUtility.UrlEncode(conditionString));
                //byte[] bs = Encoding.ASCII.GetBytes(GenerateParamString("rptdescp={0}&rptName={1}&formatType={2}&exportType=DOWNLOAD&conditions={3}", formparams));
                string parm = GenerateParamString("rptdescp={0}&rptName={1}&formatType={2}&exportType=DOWNLOAD&conditions={3}", formparams);
                string arg_count = Request["arg_count"];
                int count = 0;
                if (!string.IsNullOrEmpty(arg_count))
                    int.TryParse(arg_count, out count);
                for (int i = 0; i < count; i++)
                {
                    string arg = Request.Params["arg" + i];
                    string val = Request.Params["val" + i];
                    if (string.IsNullOrEmpty(arg) && string.IsNullOrEmpty(val))
                        continue;
                    parm += string.Format("&{0}={1}", arg, val);
                }

                byte[] bs = Encoding.ASCII.GetBytes(parm);
                OperationUtils.Logger.WriteLog("DownloadFile=" + baseUrl + Request.ApplicationPath.TrimEnd('/') + "/zh-CN/Report/CreateNewReport2Edoc");
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(baseUrl + Request.ApplicationPath.TrimEnd('/') + "/zh-CN/Report/CreateNewReport2Edoc");
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = bs.Length;
                req.Timeout = m_Timeout;
                //req.ContentType = "application/json; charset=utf-8";


                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                }

                string text;
                using (WebResponse wr = req.GetResponse())
                {
                    using (Stream stream = wr.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        text = reader.ReadToEnd();
                        string v3DownloadRptUrl = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<V3Result>(text).URL.TrimStart('/');
                        OperationUtils.Logger.WriteLog("v3DownloadRptUrl=" + v3DownloadRptUrl);
                        switch (exportFileType.ToLower())
                        {
                            case "xls":
                                {
                                    tempExportFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/" + reportTitle + ".xls";
                                    break;
                                }
                            case "doc":
                                {
                                    tempExportFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/" + reportTitle + ".docx";
                                    break;
                                }
                            case "pdf":
                                {
                                    tempExportFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/" + reportTitle + ".pdf";
                                    break;
                                }
                            default:
                                {
                                    tempExportFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/" + reportTitle + ".xls";
                                    exportFileType = "excel";
                                    break;
                                }
                        }
                        OperationUtils.Logger.WriteLog("tempExportFile=" + tempExportFile);
                        OperationUtils.Logger.WriteLog("DownloadFile=" + baseUrl + v3DownloadRptUrl);

                        if (combineCount >= Int32.Parse(item["syncDnCount"].ToString()))
                        {
                            result.Success = true;
                            result.Message = "thread";

                            Thread tr = new Thread(() => doUpload2EDOC(false ,item, edocFGUID, fileType, remark, baseUrl, v3DownloadRptUrl, tempExportFile,serverNum));

                            try
                            {
                                
                                tr.Start();

                            }
                            catch (Exception ex)
                            {

                            }


                        }
                        else
                        {
                            result = doUpload2EDOC(true, item, edocFGUID, fileType, remark, baseUrl, v3DownloadRptUrl, tempExportFile, serverNum);
                            System.IO.File.Delete(tempExportFile);
                        }
                        //删除下载的报表模板，和导出的文件 
                       
                        //XMLSerializeHelper.Deserialize<EDOCResultNewFID>(reader.ReadToEnd());
                        if (IsGuidByParse(jobNo))
                            UpdateDnMsg(jobNo, groupId, cmp, "Y");
                    }
                }
            }
            catch (Exception e)
            {
                OperationUtils.Logger.WriteLog(e.Message);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public Result doUpload2EDOC(bool sync, Dictionary<string, object> item, string edocFGUID ,string fileType , string remark, string baseUrl, string v3DownloadRptUrl, string tempExportFilePath,int serverNum)
        {

            if (!sync)
            {
                Thread.Sleep(Int32.Parse(item["syncTime"].ToString()));
            }

            Result result = new Result();

            using (System.Net.WebClient webClient = new System.Net.WebClient())
            {
                webClient.DownloadFile(new Uri(baseUrl + v3DownloadRptUrl), tempExportFilePath);
            }
            var token = UploadFileToStorage(edocFGUID, tempExportFilePath, UserId, serverNum);

            OperationUtils.Logger.WriteLog("EdocUrl=" + item["url"].ToString() + "apis/apilaunchfile.ashx?token=" + token);
            if (token != "")
            {
                string[] fileID = token.ToString().Split(new string[] { "&i=" }, StringSplitOptions.None);
                List<UpdateFileItem> fileList = new List<UpdateFileItem>();

                fileList.Add(new UpdateFileItem
                {
                    FileID = fileID[1],
                    EdocType = fileType,
                    Remark = remark
                });

                bool isSuccess = _api.UpdateFiles(fileList, serverNum);

                result.EdocUrl = item["url"].ToString() + "apis/apilaunchfile.ashx?token=" + token;
                result.Success = true;
                result.Message = "done.";
            }
            else
            {
                result.Success = false;
                result.Message = "Upload to Edoc error";
            }

            return result;
        }

        public ActionResult DoDownLoadFileToDir()
        {
            string jobNo = Request.Params["JOBNO"];
            string fileNo = Request.Params["FILENO"];
            string groupId = Request.Params["GROUP_ID"];
            string cmp = Request.Params["CMP"];
            string dep = "";// Request.Params["DEP"];
            string type = Request.Params["TYPE"];
            bool isInbound = false;
            string inbound = Request.Params["Inbound"];
            if (!string.IsNullOrEmpty(inbound))
            {
                if (inbound == "Y")
                    isInbound = true;
            }
            UserInfo userinfo = new UserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = GroupId,
                Upri = UPri,
                Dep = Dep
            };
            Thread tr = new Thread(() =>Business.DownLoadFile.DownLoadFileToDir(jobNo, fileNo, cmp, dep, type, isInbound, userinfo));

            try
            {
                tr.Start();
            }
            catch (Exception ex)
            {
            }
            return Json("true");
        }

       
        private string UploadFileToStorage(string edocFGUID, string upLoadFile, string UserId, int serverNum)
        {

            
            Dictionary<string, object> item = null;
            item = new Dictionary<string, object>();
            item["url"] = WebConfigurationManager.AppSettings["EDOC_URL"];
            item["softID"] = WebConfigurationManager.AppSettings["EDOC_SOFTID"];
            item["account"] = WebConfigurationManager.AppSettings["EDOC_ACCOUNT"];
            item["password"] = WebConfigurationManager.AppSettings["EDOC_PASSWORD"];

            //EDOCAgent.Agent.SetServerAddr(item["url"].ToString(), item["softID"].ToString(), item["account"].ToString(), item["password"].ToString());
            //EDOCResultUploadFile result = EDOCAgent.Agent.DoUploadFile(edocFGUID, upLoadFile, Path.GetFileName(upLoadFile), "Remark", UserId, "D", "8");
            EDOCResultUploadFile result = _api.UploadFile(edocFGUID, upLoadFile, "Remark", UserId, "D", "8", serverNum);
            if (result.Status == DBErrors.DB_SUCCESS)
            {
                return result.FileInfo.Token + "&i=" + result.FileInfo.FileID;
            }
            else
            {
                OperationUtils.Logger.WriteLog(result.Status.ToString());
                return "";
            }
        }

        /// <summary>
        /// 取得选择选项
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSelectOptions()
        {
            return Json(GetSelects());
        }

        public class EdocTypeOptions
        {
            public List<OptionsItem> Edt = new List<OptionsItem>();
        }

        public EdocTypeOptions GetSelects()
        {
            string codition = "";
            if (BaseStation != "*")
            {
                codition += " AND STN = " + SQLUtils.QuotedStr(BaseStation);
            }
            string sql = "SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND ((CMP=" + SQLUtils.QuotedStr(BaseCompanyId) + codition + ") OR CMP='*') AND CD_TYPE = 'EDT'";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            EdocTypeOptions iOptions = new EdocTypeOptions();
            string cd, cdDescp, cdType;
            string edtPri = GetBtnPms("EDOC")+"|";

            foreach (DataRow dr in dt.Rows)
            {
                
                cd = Prolink.Math.GetValueAsString(dr["CD"]);
                cdDescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
                cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]);
                if (edtPri != "|")
                {
                    if (edtPri.IndexOf("EDOC_EDT_V_"+cd + "|") == -1)
                    {
                        continue;
                    }
                }
                switch (cdType)
                {
                    case "EDT":
                        {
                            iOptions.Edt.Add(new OptionsItem
                            {
                                cd = cd,
                                cdDescp = cdDescp
                            });
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

            }

            return iOptions;
        }
        private string GenerateParamString(String Name, NameValueCollection values)
        {
            if (!String.IsNullOrEmpty(Name))
            {
                foreach (string key in values.Keys)
                {
                    Name = Name.Replace(key, values[key]);
                }
            }
            return Name;
        }

        /// <summary>
        /// 删除EDOC时，记录到异常管理
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="jobNo"></param>
        public void InsertTMEXP(string itemID, string jobNo)
        {
            string sql = string.Format("SELECT TOP 1 DummyName FROM FILES WHERE FileID={0}", SQLUtils.QuotedStr(itemID));
            string fileName = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string shipmentId = string.Empty;
            if (IsGuidByParse(jobNo))
            {
                sql = string.Format("SELECT TOP 1 SHIPMENT_ID FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo));
                shipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (string.IsNullOrEmpty(shipmentId))
                {
                    sql = string.Format("SELECT TOP 1 SHIPMENT_ID FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo));
                    shipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                if (string.IsNullOrEmpty(shipmentId))
                {
                    sql = string.Format("SELECT TOP 1 DN_NO FROM SMINM WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo));
                    shipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                if (string.IsNullOrEmpty(shipmentId))
                {
                    sql = string.Format("SELECT TOP 1 DN_NO FROM SMIDN WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo));
                    shipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                if (string.IsNullOrEmpty(shipmentId))//费用报价
                {
                    sql = string.Format("SELECT TOP 1 QUOT_NO FROM SMQTM WHERE U_ID ={0}", SQLUtils.QuotedStr(jobNo));
                    shipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                }
                if (string.IsNullOrEmpty(shipmentId))//账单管理
                {
                    sql = string.Format("SELECT TOP 1 TPV_DEBIT_NO FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo));
                    shipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }

            MixedList ml = new MixedList();
            Business.TmexpHandler th = new Business.TmexpHandler();
            Business.TmexpInfo tpi = new Business.TmexpInfo();
            tpi.UFid = jobNo;
            tpi.WrId = UserId;
            tpi.WrDate = TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId);
            tpi.Cmp = CompanyId;
            tpi.GroupId = GroupId;
            tpi.JobNo = shipmentId;
            tpi.ExpType = "DOC";
            tpi.ExpReason = "EDOC_DEL";
            tpi.ExpText = "Inbound," + @Resources.Locale.L_DoEdocAction + UserId + Resources.Locale.L_EdocDelete + fileName;
            tpi.ExpObj = UserId;
            ml.Add(th.SetTmexpEi(tpi));
            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        /// <summary>
        /// 获取白名单设定
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>true:符合,false:不符合</returns>
        public bool GetFileExtensions(ref string fileName)
        {
            fileName = fileName.Replace(",", "_").Replace("，", "_").Replace(";", "_").Replace("；", "_");
            string FileExtensions = Path.GetExtension(fileName);//文件后缀名，包含"."
            string sql = string.Format("SELECT TOP 1 CD_DESCP FROM BSCODE WHERE CD='EDOC' AND CD_TYPE='EFES'");
            string edocFiles = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (string.IsNullOrEmpty(edocFiles))
                return false;
            string[] listFile = edocFiles.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (!listFile.Contains(FileExtensions.Split('.')[1].ToLower()))
                return false;
            return true;
        }
    }
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string EdocUrl { get; set; }
    }

    public class V3Result
    {
        public string URL { get; set; }
    }
}
