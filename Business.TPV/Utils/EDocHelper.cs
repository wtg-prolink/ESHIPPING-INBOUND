using EDOCApi;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Business.TPV.Utils
{
    public static class EDocHelper
    {
        private static string cloumns = "FOLDER_GUID,SERVER_NUM";
        public static EDOCResultUploadFile UploadFile2EDOC(EDocInfo info)
        {
            Prolink.EDOC_API _api = new Prolink.EDOC_API();
            EDOCResultUploadFile uploadResult = null;
            int serverNum = 0;
            _api.Login();
            string folderID = CheckDuplicatedFolder(_api, info,ref serverNum);
            if (string.IsNullOrEmpty(folderID))
            {
                uploadResult = new EDOCResultUploadFile();
                uploadResult.Status = DBErrors.DB_ERR_UNEXCEPTEDERROR;
                return uploadResult;
            }
            uploadResult = _api.UploadFile(folderID, info.FilePath, info.Remark, info.UserId, "D", "8", serverNum);
            if (uploadResult == null || uploadResult.Status != DBErrors.DB_SUCCESS) return uploadResult;
            List<UpdateFileItem> fileList = new List<UpdateFileItem>();
            fileList.Add(new UpdateFileItem
            {
                FileID = uploadResult.FileInfo.FileID,
                EdocType = info.DocType,
                Remark = string.IsNullOrEmpty(info.Remark) ? "ESP" : info.Remark
            });
            _api.UpdateFiles(fileList, serverNum);
            return uploadResult;
        }

        private static string CheckDuplicatedFolder(Prolink.EDOC_API _api, EDocInfo info, ref int serverNum)
        {
            return CheckDuplicatedFolder(_api, info.JobNo, info.GroupId, info.Cmp, info.Stn, info.Dep, ref serverNum);
        }

        public static string CheckDuplicatedFolder(Prolink.EDOC_API _api, string job_no, string groupID, string cmp, string stn, string dep, ref int serverNum)
        {
            string folderID = "";
            try
            {
                folderID = _api.GetFolderGUID(job_no, groupID, cmp, stn, dep, ref serverNum, cloumns);
                if (string.IsNullOrEmpty(folderID))
                {
                    folderID = _api.SetNewFolder(ref serverNum); //沒查到才向EDOC索取並寫進table
                    if (!string.IsNullOrEmpty(folderID))
                        _api.SetFolderIDInDB(job_no, folderID, groupID, cmp, stn, dep, true, serverNum);
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
            return folderID;
        }

        public static List<FileItemInfo> GetFileItems(EDocInfo info)
        {
            Prolink.EDOC_API api = new Prolink.EDOC_API();
            api.Login();
            int serverNum = 0;
            string guid = CheckDuplicatedFolder(api, info, ref serverNum);
            List<EDOCFileItem> items = api.Inquery(guid, info.DocType, serverNum);
            if (items == null || items.Count <= 0) return null;
            return items.Select(item => new FileItemInfo(item)).ToList();
        }
    }

    public class FileItemInfo
    {
        public FileItemInfo(EDOCFileItem item)
        {
            Item = item;
        }

        public string Url
        {
            get
            {
                if (Item == null) return null;
                return string.Format("{0}apis/apilaunchfile.ashx?token={1}&i={2}", ConfigurationManager.AppSettings["EDOC_URL1"], Item.Token, Item.FileID);
            }
        }
        public EDOCFileItem Item { get; private set; }
    }

    public class EDocInfo
    {
        public string JobNo { get; set; }
        public string FilePath { get; set; }
        public string GroupId { get; set; }
        public string Cmp { get; set; }
        public string Stn { get; set; }
        public string Dep { get; set; }
        public string UserId { get; set; }
        public string DocType { get; set; }
        public string Remark { get; set; }
    }
}
