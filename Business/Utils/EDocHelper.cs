using EDOCApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Utils
{
    public static class EDocHelper
    {
        public static EDOCResultUploadFile UploadFile2EDOC(EDocInfo info)
        {
            Prolink.EDOC_API _api = new Prolink.EDOC_API();
            EDOCResultUploadFile uploadResult = null;
            _api.Login();
            string folderID = CheckDuplicatedFolder(_api, info);
            uploadResult = _api.UploadFile(folderID, info.FilePath, info.Remark, info.UserId, "D", "8");
            if (uploadResult == null || uploadResult.Status != DBErrors.DB_SUCCESS) return uploadResult;
            List<UpdateFileItem> fileList = new List<UpdateFileItem>();
            fileList.Add(new UpdateFileItem
            {
                FileID = uploadResult.FileInfo.FileID,
                EdocType = info.DocType,
                Remark = ""
            });
            _api.UpdateFiles(fileList);
            return uploadResult;
        }

        private static string CheckDuplicatedFolder(Prolink.EDOC_API _api, EDocInfo info)
        {
            string folderID = _api.GetFolderGUID(info.JobNo,info.GroupId,info.Cmp,info.Stn,info.Dep);
            if (string.IsNullOrEmpty(folderID))
            {
                folderID = _api.SetNewFolder(); //沒查到才向EDOC索取並寫進table
                if (!string.IsNullOrEmpty(folderID))
                    _api.SetFolderIDInDB(info.JobNo, folderID, info.GroupId, info.Cmp, info.Stn, info.Dep);
            }
            return folderID;
        }
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
