using Prolink.Data;
using Prolink.Persistence;
using Prolink.V6.Persistence;
using Prolink.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.Utils
{
    public class DBManager
    {
        public static List<ColumnInfo> QueryColumnInfo(List<string> tableNames)
        {
            DBModes mode = Context.GetDBModes();
            string sql = string.Empty;
            switch (mode)
            {
                case DBModes.SqlServer: sql = string.Format("SELECT CHARACTER_MAXIMUM_LENGTH,COLUMN_NAME,TABLE_NAME,DATA_TYPE from information_schema.columns where TABLE_NAME IN({0})",
                     string.Join(",", tableNames.Select(name => SQLUtils.QuotedStr(name)).ToArray())); break;
                case DBModes.Oracle: sql = string.Format("SELECT DATA_LENGTH,COLUMN_NAME,TABLE_NAME,DATA_TYPE FROM USER_TAB_COLS WHERE TABLE_NAME IN({0})",
               string.Join(",", tableNames.Select(name => SQLUtils.QuotedStr(name)).ToArray())); break;
            }
            if (string.IsNullOrEmpty(sql)) return null;
            DataTable dt = Prolink.V6.Persistence.DatabaseFactory.GetDefaultDatabase().GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            List<ColumnInfo> list = new List<ColumnInfo>();
            switch (mode)
            {
                case DBModes.SqlServer:
                    foreach (DataRow row in dt.Rows)
                    {
                        string dataType = Prolink.Math.GetValueAsString(row["DATA_TYPE"]);
                        bool isDateType = false;
                        bool isVarchar = false;
                        if (!string.IsNullOrEmpty(dataType))
                        {
                            dataType = dataType.ToLower();
                            if (dataType.IndexOf("date") != -1 || dataType.IndexOf("time") != -1)
                                isDateType = true;
                            if (dataType.IndexOf("varchar") != -1)
                                isVarchar = true;
                        }
                        list.Add(new ColumnInfo
                        {
                            ID = Prolink.Math.GetValueAsString(row["COLUMN_NAME"]),
                            TableName = Prolink.Math.GetValueAsString(row["TABLE_NAME"]),
                            Length = Prolink.Math.GetValueAsInt(row["CHARACTER_MAXIMUM_LENGTH"]),
                            IsDateType = isDateType,
                            IsVarchar = isVarchar
                        });
                    }
                    break;
                case DBModes.Oracle:
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(new ColumnInfo
                        {
                            ID = Prolink.Math.GetValueAsString(row["COLUMN_NAME"]),
                            TableName = Prolink.Math.GetValueAsString(row["TABLE_NAME"]),
                            Length = Prolink.Math.GetValueAsInt(row["DATA_LENGTH"]),
                            IsDateType = Prolink.Math.GetValueAsString(row["DATA_TYPE"]) == "DATE"
                        });
                    }
                    break;
            }
            return list;
        }

        public static void BulidDatabaseFactory()
        {
            Database db = DatabaseFactory.GetDefaultDatabase();
            if (db == null)
            {
                Prolink.V6.Persistence.Factory.Build(System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, @"db\Company.xml"));
            }
        }

        public static Database DefaultDB
        {
            get
            {
                BulidDatabaseFactory();
                return DatabaseFactory.GetDefaultDatabase();
            }
        }

        public static string CreateCondition(IEnumerable<ConditionItem> items, bool skipNullOrEmpty = true)
        {
            List<string> conditions = new List<string>();
            Func<ConditionItem, string> getConditon = item =>
            {
                if (skipNullOrEmpty && string.IsNullOrEmpty(item.Value)) return null;
                if (item.Value == null)
                    return string.Format("{0} IS NULL", item.Column);
                else
                    return string.Format("{0}={1}", item.Column, SQLUtils.QuotedStr(item.Value));
            };
            Action<ConditionItem> addConditon = item =>
            {
                string val = getConditon(item);
                if (!string.IsNullOrEmpty(val) && !conditions.Contains(val))
                {
                    conditions.Add(val);
                }
            };
            foreach (var item in items)
                addConditon(item);
            return conditions.Count <= 0 ? string.Empty : string.Join(" AND ", conditions);
        }
    }

    public class ConditionItem
    {
        public ConditionItem(string column, string vlaue)
        {
            Column = column;
            Value = vlaue;
        }
        public string Column { get; private set; }
        public string Value { get; private set; }
    }

    public class ColumnInfo
    {
        public string TableName { get; set; }
        public string ID { get; set; }
        public int Length { get; set; }
        public bool IsDateType { get; set; }
        public bool IsVarchar { get; set; }
    }
}
