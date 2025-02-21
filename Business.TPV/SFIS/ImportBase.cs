using Business.Import;
using Business.TPV.Import;
using Prolink.DataOperation;
using Prolink.Task;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.SFIS
{
    abstract class ImportBase : TpvFtpImportForLineText
    {
        protected ImportBase(string location)
        {
            Location = location;
        }

        protected string Location { get; private set; }

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
                }).First();
        }

        protected override string FileName
        {
            get { return "SFIS"; }
        }
        protected override string ConfigNodeName
        {
            get
            {
                switch (Mode)
                {
                    case ImportModes.ProductionLine: return "ProductionLineInfoImport";
                    case ImportModes.SerialNumber: return "SerialNumberInfoImport";
                }
                return null;
            }
        }

        protected abstract ImportModes Mode { get; }

        protected abstract EditInstructList CreateEiList(IEnumerable<string> lines, FtpImportEvertArgs args);

        protected override bool OperateFile(FtpImportEvertArgs args)
        {
            EditInstructList eiList = CreateEiList(CreateLines(args.LocalFileName), args);
            DatabaseFactory.GetDefaultDatabase().ExecuteUpdate(eiList);
            return true;
        }

        protected bool Update(DataTable table, string commandText)
        {
            if (table == null) return false;
            var conn = DatabaseFactory.GetDefaultDatabase().GetConnection().GetSqlConnection();
            if (conn == null) return false;
            SqlCommand comm = conn.CreateCommand();
            comm.CommandType = CommandType.Text;
            comm.CommandText = commandText;
            SqlDataAdapter adapter = new SqlDataAdapter(comm);
            SqlCommandBuilder commandBulider = new SqlCommandBuilder(adapter);
            commandBulider.ConflictOption = ConflictOption.OverwriteChanges;
            try
            {
                conn.Open();
                adapter.UpdateBatchSize = 5000;
                adapter.SelectCommand.Transaction = conn.BeginTransaction();
                adapter.Update(table.GetChanges());
                adapter.SelectCommand.Transaction.Commit();/////提交事务 
                return true;
            }
            catch (Exception ex)
            {
                if (adapter.SelectCommand != null && adapter.SelectCommand.Transaction != null)
                {
                    adapter.SelectCommand.Transaction.Rollback();
                } throw ex;
            }
            finally
            {
                conn.Close(); conn.Dispose();
            }
        }
    }

    enum ImportModes { SerialNumber, ProductionLine };
}
