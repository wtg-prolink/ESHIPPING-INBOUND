using Business.Log;
using Business.Service;
using Business.Utils;
using Prolink.DataOperation;
using Prolink.Log;
using Prolink.Persistence;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;

namespace Business
{
    public class ManagerBase
    {
        protected Database DB
        {
            get
            {
                return Business.Utils.DBManager.DefaultDB;
            }
        }

        protected DLogger Logger
        {
            get
            {
                return Business.Utils.Context.Logger;
            }
        }

        protected virtual List<string> GetCheckTables()
        {
            return null;
        }

        List<ColumnInfo> _columnInfos;
        void InitCheck()
        {
            if (_columnInfos == null)
            {
                List<string> tables = GetCheckTables();
                if (tables == null || tables.Count <= 0) return;
                _columnInfos = DBManager.QueryColumnInfo(tables);
            }
        }

        protected virtual List<ColumnInfo> GetColumnsInfos()
        {
            InitCheck();
            return _columnInfos;
        }

        protected void CheckValue(EditInstructList eiList)
        {
            var items = GetColumnsInfos();
            if (items == null || items.Count <= 0) return;
            for (int i = 0; i < eiList.Count; i++)
            {
                EditInstruct ei = eiList[i];
                CheckValue(ei);
            }
        }
        protected void CheckValue(EditInstruct ei)
        {
            var items = GetColumnsInfos();
            if (items == null || items.Count <= 0) return;
            string table = ei.ID;
            string[] columns = ei.getNameSet();
            foreach (var column in columns)
            {
                ColumnInfo columnInfo = _columnInfos.Where(info => info.ID == column && info.TableName == table).FirstOrDefault();
                if (columnInfo == null) continue;
                if (!columnInfo.IsVarchar) continue;
                string value = ei.Get(column);
                if (value.Contains(" "))//这里空格是不正常空格，是某个特殊字符  问题单：110781
                {
                    value = value.Replace(" ", " ");
                    ei.Put(column, value);
                }
                if (columnInfo.Length >= value.Length) continue;
                ei.Put(column, value.Substring(0, columnInfo.Length));
            }
        }
        void CheckValue(MixedList mixedList)
        {
            var items = GetColumnsInfos();
            if (items == null || items.Count <= 0) return;
            for (int i = 0; i < mixedList.Count; i++)
            {
                object obj = mixedList[i];
                if (!(obj is EditInstruct)) continue;
                CheckValue(obj as EditInstruct);
            }
        }

        protected ResultInfo Execute(EditInstruct ei)
        {
            if (ei == null) return SucceedResult();
            EditInstructList eiList = new EditInstructList();
            eiList.Add(ei);
            return Execute(eiList);
        }
        protected ResultInfo Execute(EditInstructList eiList)
        {
            try
            {
                CheckValue(eiList);
                DB.ExecuteUpdate(eiList);
                return SucceedResult();
            }
            catch (Exception ex)
            {
                var l = Logger.CreateLog("执行DB写入发生异常", this.GetType().Name,"", "",ex.ToString());
                Logger.WriteLog(l);
                return UnknowResult(ex);
            }
        }

        protected Func<ResultInfo> SucceedResult = () => ResultInfo.SucceedResult();
        protected Func<string, ResultInfo> NoneConfigResult = configMode =>
        {
            return new ResultInfo
            {
                IsSucceed = false,
                ResultCode = "ConfigError",
                Description = string.Format("{0}系统未配置!", configMode)
            };
        };
        protected ResultInfo UnknowResult()
        {
            return UnknowResult("Unknow");
        }
        protected ResultInfo UnknowResult(Exception ex)
        {
            return UnknowResult(ex.Message);
        }
        protected ResultInfo UnknowResult(string msg)
        {
            return ResultInfo.UnknowResult(msg);
        }
        protected ResultInfo Execute(EditInstructList eiList, int exLenght)
        {
            ResultInfo result = null;
            if (exLenght <= 0) exLenght = 100;
            EditInstructList newEiList = new EditInstructList();
            for (int i = 0; i < eiList.Count; i++)
            {
                newEiList.Add(eiList[i]);
                if (newEiList.Count == exLenght || i == eiList.Count - 1)
                {
                    result = Execute(newEiList);
                    newEiList = new EditInstructList();
                }
            }
            return result;
        }

        protected ResultInfo Execute(MixedList mixedList)
        {
            try
            {
                CheckValue(mixedList);
                DB.ExecuteUpdate(mixedList);
                return SucceedResult();
            }
            catch (Exception ex)
            {
                var l = Logger.CreateLog("执行DB写入发生异常", this.GetType().Name, "", "", ex.ToString());
                Logger.WriteLog(l);
                return UnknowResult(ex);
            }
        }


        public virtual bool Check<T>(IEnumerable<T> objs, ref EntityValidationResult result) where T : class
        {
            foreach (var item in objs)
            {
                result = ValidationHelper.ValidateEntity(item);
                if (result.HasError) return false;
            }
            return true;
        }

        protected string BackupData(IEnumerable<string> dirPath, XmlDocument doc, string fileName = "")
        {
            return BackupXml(dirPath, doc.InnerXml, fileName);
        }

        protected string BackupXml(IEnumerable<string> dirPath, string xml, string fileName = "")
        {
            if (string.IsNullOrEmpty(fileName)) fileName = GetCurrentTimeString();
            string backupFileName = CreateBaseDirectoryFileName(dirPath, fileName, "xml");
            BackupData(backupFileName, xml);
            return backupFileName;
        }

        protected string GetCurrentTimeString()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        protected void BackupData(string filePath, string txt)
        {
            ExportToFile(filePath, txt);
        }
        protected string BackupData(object obj)
        {
            return BackupData(obj, GetCurrentTimeString());
        }
        protected string BackupData(object obj, IEnumerable<string> dirPath, string fileName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(fileName)) fileName = GetCurrentTimeString();
                string path = string.Empty;
                if (dirPath == null)
                    path = CreateFileName(obj, fileName);
                else path = CreateBaseDirectoryFileName(dirPath, fileName, "txt");
                string txt = ToJsonString(obj);
                BackupData(path, txt);
                return path;
            }
            catch (Exception ex)
            {
                var l = Logger.CreateLog("备份数据失败", this.GetType().Name, "", "", ex.ToString());
                Logger.WriteLog(l);
                return null;
            }
        }
        protected string BackupData(object obj, string fileName)
        {
            return BackupData(obj, null, fileName);
        }
        public string ToJsonString(object obj)
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            return Serializer.Serialize(obj);
        }
        protected const string BackupDirName_Import = "Import";
        protected const int BackDirDate = 30;
        string CreateFileName(object obj, string fileName)
        {
            return CreateBaseDirectoryFileName(new List<string> { BackupDirName_Import, obj.GetType().Name }, fileName);
        }
        protected string CreateBaseDirectoryFileName(IEnumerable<string> modes, string fileName, string extension = "txt")
        {
            List<string> items = new List<string> { AppDomain.CurrentDomain.BaseDirectory, "../" };
            items.AddRange(modes);
            try
            {
                DateTime to = DateTime.Now;
                DateTime from = to.AddDays(-BackDirDate);
                string logDir = Path.Combine(items.ToArray());
                string[] dirs = System.IO.Directory.GetDirectories(logDir);
                for (int i = 0; i < dirs.Length; i++)
                {
                    System.IO.DirectoryInfo dirinf = new System.IO.DirectoryInfo(dirs[i]);
                    if (dirinf.CreationTime < from)
                    {
                        System.IO.Directory.Delete(dirs[i], true);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            items.AddRange(new List<string> { DateTime.Now.ToString("yyyyMMdd"), string.Format("{0}.{1}", fileName, extension) });
            return Path.Combine(items.ToArray());
        }

        protected void CreateDir(string filename)
        {
            string filePath = Path.GetDirectoryName(filename);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
        }

        void ExportToFile(string filename, string txt)
        {
            CreateDir(filename);
            var permissionSet = new PermissionSet(PermissionState.None);
            var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, filename);
            permissionSet.AddPermission(writePermission);
            if (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
            {
                using (FileStream fstream = new FileStream(filename, FileMode.Create))
                using (TextWriter writer = new StreamWriter(fstream))
                {
                    writer.WriteLine(txt);
                }
            }
            else
            {
                throw new Exception(string.Format("the file don't have write permission!:{0}", filename));
            }
        }

    }
}
