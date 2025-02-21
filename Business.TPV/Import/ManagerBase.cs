using Business.TPV.RFC;
using Business.Utils;
using Models.EDI;
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

namespace Business.TPV.Import
{
    public class ManagerBase
    {
        protected Database DB
        {
            get
            {
                Business.Utils.DBManager.BulidDatabaseFactory();
                return DatabaseFactory.GetDefaultDatabase();
            }
        }

        DefaultLogger _logger;
        protected DefaultLogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../Log", this.GetType().Name);
                    _logger = new DefaultLogger(path);
                }
                return _logger;
            }
        }

        protected virtual List<string> GetCheckTables()
        {
            return null;
        }

        static List<ColumnInfo> _columnInfos;
        void InitCheck()
        {
            if (_columnInfos == null)
            {
                List<string> tables = GetCheckTables();
                if (tables == null || tables.Count <= 0) return;
                _columnInfos = DBManager.QueryColumnInfo(tables);
            }
        }
        void CheckValue(EditInstructList eiList)
        {
            InitCheck();
            if (_columnInfos == null || _columnInfos.Count <= 0) return;
            for (int i = 0; i < eiList.Count; i++)
            {
                EditInstruct ei = eiList[i];
                CheckValue(ei);
            }
        }
        void CheckValue(EditInstruct ei)
        {
            InitCheck();
            if (_columnInfos == null || _columnInfos.Count <= 0) return;
            string table = ei.ID;
            string[] columns = ei.getNameSet();
            foreach (var column in columns)
            {
                ColumnInfo columnInfo = _columnInfos.Where(info => info.ID == column && info.TableName == table).FirstOrDefault();
                if (columnInfo == null) continue;
                if (!columnInfo.IsVarchar) continue;
                string value = ei.Get(column);
                if (columnInfo.Length >= value.Length) continue;
                ei.Put(column, value.Substring(0, columnInfo.Length));
            }
        }
        void CheckValue(MixedList mixedList)
        {
            InitCheck();
            if (_columnInfos == null || _columnInfos.Count <= 0) return;
            for (int i = 0; i < mixedList.Count; i++)
            {
                object obj = mixedList[i];
                if (!(obj is EditInstruct)) continue;
                CheckValue(obj as EditInstruct);
            }
        }
        protected ResultInfo Execute(EditInstructList eiList)
        {
            try
            {
                CheckValue(eiList);
                DB.ExecuteUpdate(eiList);
                return new ResultInfo { IsSucceed = true, ResultCode = ResultCode.Succeed, Description = ResultCode.GetDescription(ResultCode.Succeed) };
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new ResultInfo { IsSucceed = false, ResultCode = ResultCode.UnKnow, Description = ex.Message };
            }
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
                return new ResultInfo { IsSucceed = true, ResultCode = ResultCode.Succeed, Description = ResultCode.GetDescription(ResultCode.Succeed) };
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new ResultInfo { IsSucceed = false, ResultCode = ResultCode.UnKnow, Description = ex.Message };
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

        protected void BackupData(object obj)
        {
            BackupData(obj, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        }

        protected void BackupData(object obj, string fileName)
        {
            try
            {
                string txt = ToJsonString(obj);
                string path = CreateFileName(obj, fileName);
                ExportToFile(path, txt);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(string.Format("备份数据失败: {0}", fileName), ex);
            }
        }
        string ToJsonString(object obj)
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            return Serializer.Serialize(obj);
        }
        string CreateFileName(object obj, string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../Import", obj.GetType().Name, DateTime.Now.ToString("yyyyMMdd"), string.Format("{0}.txt", fileName));
        }
        void ExportToFile(string filename, string txt)
        {
            string filePath = Path.GetDirectoryName(filename);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
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
