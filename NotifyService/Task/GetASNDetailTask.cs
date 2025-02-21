using Business.Import;
using Business.TPV.Import;
using Business.TPV.Utils;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace NotifyService.Task
{
    public class GetASNDetailTask : FtpImportBase, IPlanTask
    {
        public string _nodeName = "ASNDetail";

        public GetASNDetailTask(string hour)
        { }
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            XmlDocument doc = new XmlDocument();
            //_messenger.GetLogger().WriteLog("path:" + ConfigFileName);
            doc.Load(ConfigFileName);
            XmlNodeList nodeList = doc.DocumentElement.SelectNodes("ftp");
            //_messenger.GetLogger().WriteLog("nodelist:" + nodeList.Count);
            if (nodeList.Count > 0)
            {
                XmlNode ftpNode = nodeList[0];
                foreach (XmlNode node in ftpNode.ChildNodes)
                {
                    string name = node.Name;
                    if (!_nodeName.Equals(name))
                        _nodeName = name;
                    this.AnalyzeFile();
                }
            }
        }
        protected string ConfigNodeName
        {
            get
            {
                return _nodeName;
            }
        }
        string Location = "";
        protected string ConfigFileName
        {
            get
            {
                return System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, string.Format("edi/ftp/ASN.xml"));
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
                    c.FtpBackupFlag = string.Format(c.FtpBackupFlag, Location);
                }
                c.SearchDir = "Y";
            }).First();
        }

        protected override bool OperateFile(FtpImportEvertArgs e)
        {
            bool flag = true;
            XmlDocument doc = new XmlDocument();
            string xml = "";
            using (StreamReader sr = new StreamReader(e.LocalFileName))
            {
                //This allows you to do one Read operation.
                xml = sr.ReadToEnd();
            }
            EdiInfo ediinfo = new EdiInfo();
            string fileName = System.IO.Path.GetFileNameWithoutExtension(e.LocalFileName);
            bool isDirect = "DD_".Equals(fileName.Substring(0, 3));
            string[] files = fileName.Split('_');
            string invNo = "";
            if (files.Length > 4)
            {
                invNo = files[3];
            }
            if (isDirect && files.Length > 2)
                invNo = files[1];
            ediinfo.DataFolder = e.LocalFileName.Replace(e.Config.DownloadPath, CreateDirectory(Path.Combine(e.Config.BackupPath, DateTime.Now.ToString("yyyyMMdd"))));
            ediinfo.CreateBy = "System Auto";
            ediinfo.FromCd = "ftp";
            ediinfo.ToCd = "Eshipping";
            ediinfo.Rs = "Receive";
            ediinfo.Cmp = "MN";
            ediinfo.GroupId = "TPV";
            doc.LoadXml(xml);
            ediinfo.Data = doc.InnerXml;
            switch (ConfigNodeName)
            {
                case "ASNDetail":
                    flag = ASNManager.SetAsnDetail(doc, ediinfo, isDirect, invNo);
                    break;
                case "GRDetail":
                    flag = ASNManager.SetGrDetail(doc, ediinfo);
                    break;
            }
            if(!string.IsNullOrEmpty(ediinfo.ID))
                AddEDIInfoToDB(ediinfo);
            return flag;
        }

        public void AddEDIInfoToDB(EdiInfo ediInfo)
        {
            try
            {
                MixedList ml = new MixedList();
                EditInstruct ei = new EditInstruct("EDI_LOG", Prolink.DataOperation.EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", ediInfo.ID);
                ei.Put("EDI_ID", ediInfo.EdiId);
                ei.PutExpress("EVENT_DATE", "getdate()");
                if (!string.IsNullOrEmpty(ediInfo.Remark) && ediInfo.Remark.Length > 500)
                    ediInfo.Remark = ediInfo.Remark.Substring(0, 500);
                ei.Put("REMARK", ediInfo.Remark.TrimEnd(';'));
                ei.Put("SENDER", ediInfo.CreateBy);
                ei.Put("RS", ediInfo.Rs);
                ei.Put("STATUS", ediInfo.Status);
                ei.Put("FROM_CD", ediInfo.FromCd);
                ei.Put("TO_CD", ediInfo.ToCd);
                ei.Put("DATA_FOLDER", ediInfo.DataFolder);
                ei.Put("REF_NO", ediInfo.RefNO);
                ei.Put("GROUP_ID", ediInfo.GroupId);
                ei.Put("CMP", ediInfo.Cmp);
                ei.Put("STN", ediInfo.Stn);
                ml.Add(ei);
                string edidata = Prolink.Math.GetValueAsString(ediInfo.Data);
                if (!string.IsNullOrEmpty(edidata) && ml != null)
                {
                    EditInstruct edidataei = new EditInstruct("EDI_DATA", EditInstruct.INSERT_OPERATION);
                    edidataei.Put("U_ID", ediInfo.ID);
                    edidataei.Put("EDI_DATE", edidata);
                    edidataei.PutExpress("CREATE_DATE", "getdate()");
                    ml.Add(edidataei);
                }
                int[] result = DB.ExecuteUpdate(ml);
            }
            catch (Exception e) { }
        }

    }
}
