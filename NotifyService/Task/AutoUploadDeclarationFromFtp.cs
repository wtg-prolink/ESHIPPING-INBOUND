using Business.Import;
using Business.TPV;
using Business.TPV.Utils;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using TrackingEDI.Business;

namespace NotifyService.Task
{
    public class AutoUploadDeclarationFromFtp : FtpImportBase, IPlanTask
    {
        string Location = "";
        string FileName = "Declaration";
        DataTable _decNoTable;
        protected string ConfigNodeName
        {
            get
            {
                return "Receive";
            }
        }
        protected string ConfigFileName
        {
            get
            { 
                return System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, string.Format("edi/ftp/{0}.xml", FileName));
            }
        }
        protected override Business.Utils.FTPConfig GetFtpConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigFileName);
            XmlNodeList nodeList = doc.GetElementsByTagName(ConfigNodeName);
            return Business.Utils.ConfigManager.GetFTPConfig(nodeList, c =>
            {
                if (!string.IsNullOrEmpty(Location))
                {
                    c.Path = string.Format(c.Path, Location);
                    c.LogPath = string.Format(c.LogPath, Location);
                    c.BackupPath = string.Format(c.BackupPath, Location);
                    c.DownloadPath = string.Format(c.DownloadPath, Location);
                    c.FtpBackupPath = string.Format(c.FtpBackupPath, Location);
                }
                c.SearchDir = "Y";
            }).First();
        }

        public void Run(IPlanTaskMessenger messenger)
        {
            _decNoTable= GetDeclarationTable();
            if (_decNoTable.Rows.Count <= 0) return;
            this.AnalyzeFile();
        }
        private DataTable GetDeclarationTable()
        {
            string sql= string.Format(@"DELETE FROM DECLARATION_TASK WHERE CREATE_DATE <DATEADD(month, -3, getDate()) ");
            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format(@"SELECT * FROM DECLARATION_TASK WHERE SUCCESS='N' AND (ERROR_COUNT IS NULL OR ERROR_COUNT<=5)");
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        protected override bool OperateFile(FtpImportEvertArgs e)
        { 
            string fileName = System.IO.Path.GetFileNameWithoutExtension(e.LocalFileName); 
            foreach (DataRow dr in _decNoTable.Rows)
            {
                string decInfo = Prolink.Math.GetValueAsString(dr["DEC_INFO"]);
                string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);

                string[] decNoStr = decInfo.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string decNo in decNoStr)
                {
                    if (fileName != decNo) continue;
                    string remark = string.Empty;
 
                    EDocInfo info = Helper.CreateShipmentEDocInfo(dr); 
                    string edoctype = "IBCC"; 
                    info.DocType = edoctype;
                    info.FilePath = e.LocalFileName;
                    info.Remark = "FTP"; 
                    try
                    {
                        EDOCApi.EDOCResultUploadFile uploadResult = EDocHelper.UploadFile2EDOC(info);
                        if (uploadResult == null || uploadResult.Status != EDOCApi.DBErrors.DB_SUCCESS)
                        {
                            remark = "Upload Error:" + uploadResult.Status;
                            Logger.WriteLog("Upload Error:" + "ShipmentId=" + shipmentId + ",DecNo=" + decNo + " " + uploadResult.Status
                                + "  " + info.FilePath + "  " + info.Remark + "  " + info.DocType);
                            UpdateDeclaration(dr, "N", remark);
                            return false;
                        } 
                    }
                    catch (Exception ex)
                    {
                        remark = "Operate Exception:" + ex.ToString(); 
                        Logger.WriteLog("Operate Exception:" + ex.ToString());
                        UpdateDeclaration(dr, "N", remark);
                        return false;
                    } 
                    UpdateDeclaration(dr, "Y", remark);
                } 
            }
            return true;
        }

        private void UpdateDeclaration(DataRow dr, string success, string remark)
        {
            EditInstruct ei = new EditInstruct("DECLARATION_TASK", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("NEW_ID", dr["NEW_ID"]);
            ei.Put("SUCCESS", success);
            ei.PutDate("PROCESS_DATE", DateTime.Now);
            ei.Put("REMARK", remark.Length > 500 ? remark.Substring(0, 500) : remark); 
            if ("N".Equals(success))
                ei.Put("ERROR_COUNT", Prolink.Math.GetValueAsInt(dr["ERROR_COUNT"]) + 1);
            OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
         
    }
}
