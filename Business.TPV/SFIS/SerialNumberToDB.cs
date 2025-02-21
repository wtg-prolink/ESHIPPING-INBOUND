using Business.Import;
using Business.TPV.Base;
using Business.Utils;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;

namespace Business.TPV.SFIS
{
    class SerialNumberToDB : ImportBase
    {
        public SerialNumberToDB(string location)
            : base(location)
        {
        }

        protected override ImportModes Mode
        {
            get { return ImportModes.SerialNumber; }
        }

        const string Column_ID = "U_ID";
        const string Column_FID = "U_FID";
        const string Column_Number = "SERIAL_NUMBER";
        const string Column_DN = "DN_NO";
        const string Column_ODN = "ODN_NO";
        const string Column_PartNO = "PART_NO";
        const string Column_JobNO = "JOB_NO";

        const string Column_REF_NO = "REF_NO";
        const string Column_CURRENT_ID = "CURRENT_ID";
        const string Column_SEAL_QTY = "SEAL_QTY";
        const string Column_CREATE_DATE = "CREATE_DATE";

        DataTable CreateDT()
        {
            DataTable dt = null;
            DataTable dt_temp = null;
            try
            {
                dt_temp = new DataTable();
                dt_temp.Columns.Add(Column_ID, typeof(SqlGuid));
                dt_temp.Columns.Add(Column_FID, typeof(SqlGuid));
                dt_temp.Columns.Add(Column_DN, typeof(string));
                dt_temp.Columns.Add(Column_PartNO, typeof(string));
                dt_temp.Columns.Add(Column_Number, typeof(string));
                dt_temp.Columns.Add(Column_JobNO, typeof(string));
                dt_temp.Columns.Add(Column_ODN, typeof(string));
                dt = dt_temp;
                dt_temp = null;
                return dt;
            }
            finally
            {
                if (dt_temp != null)
                    dt_temp.Dispose();
            }
        }
        DataTable CreateDT_SEAL_QTY()
        {
            DataTable dt = null;
            DataTable dt_temp = null;
            try
            {
                dt_temp = new DataTable();
                dt_temp.Columns.Add(Column_ID, typeof(SqlGuid));
                dt_temp.Columns.Add(Column_FID, typeof(SqlGuid));
                dt_temp.Columns.Add(Column_REF_NO, typeof(string));
                dt_temp.Columns.Add(Column_CURRENT_ID, typeof(SqlGuid));
                dt_temp.Columns.Add(Column_SEAL_QTY, typeof(decimal));
                dt_temp.Columns.Add(Column_CREATE_DATE, typeof(DateTime));
                dt = dt_temp;
                dt_temp = null;
                return dt;
            }
            finally
            {
                if (dt_temp != null)
                    dt_temp.Dispose();
            }
        } 

        protected override void OnAnalyzeFile()
        {
            try
            {
                string sql = string.Format(@"SELECT TOP 20 FILENAME,U_ID FROM SMDNS_FILE WHERE (FLAG IS NULL OR FLAG='N') AND 
                CMP={0} ORDER BY CREATE_DATE DESC", SQLUtils.QuotedStr(Location));
                DataTable dt = DB.GetDataTable(sql, new string[] { });
                if (dt == null || dt.Rows.Count <= 0) return;
                foreach (DataRow row in dt.Rows)
                {
                    string fileName = Prolink.Math.GetValueAsString(row["FILENAME"]);
                    if (string.IsNullOrEmpty(fileName)) continue;
                    if (!File.Exists(fileName)) continue;
                    List<SerialNumberInfo> infos = null;
                    EditInstruct ei = new EditInstruct("SMDNS_FILE", EditInstruct.UPDATE_OPERATION);
                    ei.PutExpress("ACTION_DATE", "getdate()");
                    string fileId = Prolink.Math.GetValueAsString(row["U_ID"]);
                    ei.Condition = string.Format("U_ID={0}", SQLUtils.QuotedStr(fileId));
                    bool ishandle=HandleFile(fileName, fileId, out infos);
                    if (ishandle || infos == null || infos.Count <= 0)
                    {
                        ei.Put("FLAG", "Y");
                        SerNOToDBEDILog log = new SerNOToDBEDILog(infos, fileName);
                        var v = log.CreateSucceed();
                        v.DataFolder = fileName;
                        Logger.WriteLog(v);
                    }
                    else
                    {
                        SerNOToDBEDILog log = new SerNOToDBEDILog(infos, fileName);
                        Logger.WriteLog(log.CreateEx("未处理数据！"));
                    }
                    var result = Execute(ei);
                }
            }
            catch (Exception e)
            {
                SerNOToDBEDILog log = new SerNOToDBEDILog(null, null);
                Business.Utils.Context.Logger.WriteLog(log.CreateEx(e));
            }
        }

        string GetCompletedPath(FTPConfig config)
        {
            string path = Path.Combine(config.BackupPath, "../Completed/");
            return CreateDirectory(Path.Combine(path, DateTime.Now.ToString("yyyyMMdd")));
        }

        bool DeleteOriginal(List<SerialNumberInfo> infos)
        {
            var items = infos.Select(n => n.DNNO).Distinct();
            EditInstructList eiList = new EditInstructList();
            foreach (var v in items)
            {
                EditInstruct ei = new EditInstruct("SMDNS", EditInstruct.DELETE_OPERATION);
                ei.Condition = string.Format("ODN_NO={0}", SQLUtils.QuotedStr(v));
                eiList.Add(ei);
            }
            return Execute(eiList).IsSucceed;
        }

        bool HandleFile(string filePath,string fileId, out List<SerialNumberInfo> infos)
        {
            var lines = CreateLines(filePath);
            infos = CreateSerialNumberInfo(lines).ToList();
            if (infos == null || infos.Count <= 0) return false;
            if (!DeleteOriginal(infos)) return false;
            DataTable dt = QueryCmp(infos);
            List<dynamic> objs = new List<dynamic>();
            foreach (var info in infos)
            {
                var obj = CreateObj(info, dt);
                objs.AddRange(obj);
            }
            if (objs.Count <= 0) return false;
            bool result = ToDB_DNS(objs);
            if (result)
            {
                result = UpDNSealQTY(infos, fileId);
                if (result)
                {
                    SendDNStatus(infos,Location);
                }
                return result;
            }
            return false;
        }

        void SendDNStatus(List<SerialNumberInfo> _items,string location)
        {
            if (_items == null || _items.Count <= 0) return;
            string sql = string.Format("SELECT DISTINCT ORIGIN_NO FROM SMDN WHERE QTY<=SEAL_QTY AND ORIGIN_NO IN({0})",
                string.Join(",", _items.Select(item => item.DNNO).Distinct().Select(s => SQLUtils.QuotedStr(s))));
            DataTable dt = DatabaseFactory.GetDefaultDatabase().GetDataTable(sql, new string[] { });

            sql="SELECT REF_NO FROM SMDN_SEAL_QTY WHERE REF_NO NOT IN(SELECT DISTINCT ORIGIN_NO FROM SMDN)";
            DataTable noDt = DatabaseFactory.GetDefaultDatabase().GetDataTable(sql, new string[] { });

            var items = GetDNStatusInfo(dt, noDt).Distinct().ToList();
            if (items == null || items.Count <= 0) return;
            try
            {
                string filePath = Manager.SendDNStatus(items, location);
                UpdateFlag(items);
                SendDNStatusEDILog log = new SendDNStatusEDILog(items, filePath);
                Business.Utils.Context.Logger.WriteLog(log.CreateSucceed());
            }
            catch (Exception e)
            {
                SendDNStatusEDILog log = new SendDNStatusEDILog(null, null);
                Business.Utils.Context.Logger.WriteLog(log.CreateEx(e));
            }
        }


        void UpdateFlag(List<DNStatusInfo> infos)
        {
            if (infos == null || infos.Count <= 0) return;
            string sql = string.Format("UPDATE SMDN SET EX_CD='Y', EX_NAME='' WHERE DN_NO IN({0})",
                string.Join(",", infos.Select(item => SQLUtils.QuotedStr(item.CmpDNNO))));
            DatabaseFactory.GetDefaultDatabase().ExecuteUpdate(sql);
        }

        IEnumerable<DNStatusInfo> GetDNStatusInfo(DataTable dt,DataTable noDt)
        {
            foreach (DataRow row in dt.Rows)
            {
                DNStatusInfo info = new DNStatusInfo();
                info.DNNO = Prolink.Math.GetValueAsString(row["ORIGIN_NO"]);
                info.Status = DNStatus.Finished;
                yield return info;
            }

            foreach (DataRow row in noDt.Rows)
            {
                DNStatusInfo info = new DNStatusInfo();
                info.DNNO = Prolink.Math.GetValueAsString(row["REF_NO"]);
                info.Status = DNStatus.Finished;
                yield return info;
            }
        }

        bool UpDNSealQTY(List<SerialNumberInfo> infos, string fielIid)
        {
            EditInstructList eiList = new EditInstructList();
            List<dynamic> list = new List<dynamic>();
            var currentId = System.Guid.NewGuid();
            foreach (var item in infos.GroupBy(x => x.DNNO))
            {
                var d = list.Where(x => x.refNo == item.Key).FirstOrDefault();
                if (d != null) continue;
                list.Add(new { id = (SqlGuid)System.Guid.NewGuid(), fid = new SqlGuid(fielIid), currentId = (SqlGuid)currentId, refNo = item.Key, qty = item.Count() });
            }
            if (list.Count <= 0) return false;
            DB.ExecuteUpdate("truncate table SMDN_SEAL_QTY");
            if (ToDB_DN_SEAL_QTY(list))
            {
                EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                ei.PutExpress("SEAL_QTY", string.Format("(SELECT SEAL_QTY FROM SMDN_SEAL_QTY Q WHERE Q.REF_NO=SMDN.ORIGIN_NO AND Q.CURRENT_ID={0})",
                    SQLUtils.QuotedStr(currentId.ToString())));
                ei.Condition = string.Format("SMDN.ORIGIN_NO IN(SELECT REF_NO FROM SMDN_SEAL_QTY Q WHERE Q.CURRENT_ID={0})", SQLUtils.QuotedStr(currentId.ToString()));
                return Execute(ei).IsSucceed;
            }
            return false;
        }

        DataTable CreateSourceDT_SEAL_QTY(List<dynamic> objs)
        {
            DataTable dt = CreateDT_SEAL_QTY();
            foreach (var d in objs)
            {
                DataRow row = dt.NewRow();
                row[Column_ID] = d.id;
                row[Column_FID] = d.fid;
                row[Column_REF_NO] = d.refNo;
                row[Column_CURRENT_ID] = d.currentId;
                row[Column_SEAL_QTY] = d.qty;
                row[Column_CREATE_DATE] = DateTime.Now;
                dt.Rows.Add(row);
            }
            return dt;
        }

        DataTable QueryTable(List<SerialNumberInfo> items)
        {
            if (items == null || items.Count <= 0) return null;
            List<string> list = items.Select(x => x.DNNO).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList();
            if (list == null || list.Count <= 0) return null;
            string sql = string.Format("SELECT U_ID,SEAL_QTY,ORIGIN_NO FROM SMDN WHERE {0} ",
                string.Join(" OR ", list.Select(x => string.Format("ORIGIN_NO={0}", SQLUtils.QuotedStr(x)))));
            return DB.GetDataTable(sql, new string[] { });
        }

        DataTable CreateUpTable(List<SerialNumberInfo> items)
        {
            DataTable table = QueryTable(items);
            if (table == null || table.Rows.Count <= 0) return null;
            DatabaseFactory.Logger.WriteLog("Query SMDN" + table.Rows.Count + "  " + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
            table.AcceptChanges();
            table.TableName = "SMDN";
            foreach (var item in items)
            {
                DataRow[] rows = table.Select(string.Format("ORIGIN_NO={0}", SQLUtils.QuotedStr(item.DNNO)));
                if (rows == null || rows.Length <= 0) continue;
                foreach (var row in rows)
                {
                    row["SEAL_QTY"] = items.Count(s => s.DNNO == item.DNNO);
                }
            }
            return table;
        }

        bool ToDB_DN_SEAL_QTY(List<dynamic> objs)
        {
            var conn = DatabaseFactory.GetDefaultDatabase().GetConnection().GetSqlConnection();
            if (conn == null) return false;
            DataTable dt =CreateSourceDT_SEAL_QTY(objs);
            conn.Open();
            using (SqlBulkCopy mySbc = new SqlBulkCopy(conn))
            {
                mySbc.BulkCopyTimeout = 180;
                mySbc.DestinationTableName = "SMDN_SEAL_QTY";
                mySbc.WriteToServer(dt);
                conn.Close();
                return true;
            }
        } 

        bool ToDB_DNS(List<dynamic> objs)
        {
            var conn = DatabaseFactory.GetDefaultDatabase().GetConnection().GetSqlConnection();
            if (conn == null) return false;
            DataTable dt = CreateSourceDT(objs);
            conn.Open();
            using (SqlBulkCopy mySbc = new SqlBulkCopy(conn))
            {
                mySbc.BulkCopyTimeout = 180;
                mySbc.DestinationTableName = "SMDNS";
                mySbc.WriteToServer(dt);
                conn.Close();
                return true;
            }
        }

        DataTable CreateSourceDT(List<dynamic> objs)
        {
            DataTable dt = CreateDT();
            foreach (var d in objs)
            {
                DataRow row = dt.NewRow();
                row[Column_ID] = d.id;
                row[Column_Number] = d.sn;
                row[Column_DN] = d.dn;
                row[Column_ODN] = d.odn;
                dt.Rows.Add(row);
            }
            return dt;
        }

        DataTable QueryCmp(List<SerialNumberInfo> infos)
        {
            string sql = string.Format("SELECT ORIGIN_NO,STN FROM SMDN WHERE ORIGIN_NO IN({0})", string.Join(",",
                infos.Select(item => item.DNNO).Distinct().Select(s => SQLUtils.QuotedStr(s))));
            return DatabaseFactory.GetDefaultDatabase().GetDataTable(sql, new string[] { });
        }

        IEnumerable<SerialNumberInfo> CreateSerialNumberInfo(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                string[] strs = line.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length <= 1) continue;
                yield return new SerialNumberInfo { DNNO = GetValue(strs, 0), SerialNumber = GetValue(strs, 1) };
            }
        }

        IEnumerable<dynamic> CreateObj(SerialNumberInfo info, DataTable dt)
        {
            Func<string, string> getCmp = dnno =>
            {
                if (dt == null || dt.Rows.Count <= 0) return null;
                DataRow[] rows = dt.Select(string.Format("ORIGIN_NO={0}", SQLUtils.QuotedStr(dnno)));
                if (rows == null || rows.Length <= 0) return null;
                return rows.Select(row => Prolink.Math.GetValueAsString(row["STN"])).FirstOrDefault();
            };
            var cmp = getCmp(info.DNNO);
            yield return new
            {
                id = (SqlGuid)System.Guid.NewGuid(),
                sn = info.SerialNumber,
                dn = string.IsNullOrEmpty(cmp) ? info.DNNO : string.Join(string.Empty, cmp, info.DNNO),
                odn = info.DNNO
            };
        }

        protected override Prolink.DataOperation.EditInstructList CreateEiList(IEnumerable<string> lines, FtpImportEvertArgs args)
        {
            throw new NotImplementedException();
        }
    }
}