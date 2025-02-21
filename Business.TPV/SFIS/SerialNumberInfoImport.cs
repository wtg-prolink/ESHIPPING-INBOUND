using Business.Import;
using Business.TPV.Base;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.SFIS
{
    class SerialNumberInfoImport : ImportBase
    {
        public SerialNumberInfoImport(string location)
            : base(location)
        {
            this.AfterBackup += SerialNumberInfoImport_AfterBackup;
            this.OccuredError += SerialNumberInfoImport_OccuredError;
        }

        void SerialNumberInfoImport_OccuredError(object sender, FtpImportEvertArgs e)
        {
            SFISerNOEDILog log = new SFISerNOEDILog(e);
            Logger.WriteLog(log.CreateEx(e.EX));
        }

        void SerialNumberInfoImport_AfterBackup(object sender, FtpImportEvertArgs e)
        {
            EditInstruct ei = new EditInstruct("SMDNS_FILE", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", System.Guid.NewGuid().ToString());
            ei.Put("FILENAME", e.BackupFileName);
            ei.Put("FLAG", "N");
            ei.Put("CMP", Location);
            ei.PutExpress("CREATE_DATE", "getdate()");
            var result = Execute(ei);
            SFISerNOEDILog log = new SFISerNOEDILog(e);
            Logger.WriteLog(log.CreateSucceed());
        }

        protected override ImportModes Mode
        {
            get { return ImportModes.SerialNumber; }
        }
        List<SerialNumberInfo> _items;
        protected override bool OperateFile(FtpImportEvertArgs args)
        {
            return true;
            //_items = null;
            //List<SerialNumberInfo> infos = null;
            //return HandleFile(args.LocalFileName, out infos);
        }

        const string Column_ID = "U_ID";
        const string Column_FID = "U_FID";
        const string Column_Number = "SERIAL_NUMBER";
        const string Column_DN = "DN_NO";
        const string Column_ODN = "ODN_NO";
        const string Column_PartNO = "PART_NO";
        const string Column_JobNO = "JOB_NO";


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

        //bool HandleFile(string filePath, out List<SerialNumberInfo> infos)
        //{
        //    var lines = CreateLines(filePath);
        //    infos = CreateSerialNumberInfo(lines).ToList();
        //    if (infos == null || infos.Count <= 0) return false;
        //    if (!DeleteOriginal(infos)) return false;
        //    DataTable dt = QueryCmp(infos);
        //    List<dynamic> objs = new List<dynamic>();
        //    foreach (var info in infos)
        //    {
        //        var obj = CreateObj(info, dt);
        //        objs.AddRange(obj);
        //    }
        //    if (objs.Count <= 0) return false;
        //    bool result = ToDB(objs);
        //    if (result)
        //    {
        //        SendDNStatus();
        //        return UpDNSealQTY(infos);
        //    }
        //    return false;
        //}

        bool UpDNSealQTY(List<SerialNumberInfo> infos)
        {
            DataTable table = CreateUpTable(infos);
            if (table == null) return false;
            return Update(table, "SELECT U_ID,SEAL_QTY,ORIGIN_NO FROM SMDN WHERE 1=0");
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
            table.AcceptChanges();
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

        bool ToDB(List<dynamic> objs)
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

        void SendDNStatus()
        {
            if (_items == null || _items.Count <= 0) return;
            string sql = string.Format("SELECT ORIGIN_NO FROM SMDN WHERE QTY<=SEAL_QTY AND ORIGIN_NO IN({0})",
                string.Join(",", _items.Select(item => item.DNNO).Distinct().Select(s => SQLUtils.QuotedStr(s))));
            DataTable dt = DatabaseFactory.GetDefaultDatabase().GetDataTable(sql, new string[] { });
            var items = GetDNStatusInfo(dt).Distinct().ToList();
            if (items == null || items.Count <= 0) return;
            Manager.SendDNStatus(items);
        }

        IEnumerable<DNStatusInfo> GetDNStatusInfo(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                DNStatusInfo info = new DNStatusInfo();
                info.DNNO = Prolink.Math.GetValueAsString(row["ORIGIN_NO"]);
                info.Status = DNStatus.Finished;
                yield return info;
            }
        }

        protected override EditInstructList CreateEiList(IEnumerable<string> lines, FtpImportEvertArgs args)
        {
            throw new NotImplementedException();
        }
    }

    class SerialNumberInfo
    {
        public string DNNO { get; set; }
        public string SerialNumber { get; set; }
    }
}
