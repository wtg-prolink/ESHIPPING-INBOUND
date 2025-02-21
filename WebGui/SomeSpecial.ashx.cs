using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.IO;
using System.Security;
using System.Security.Permissions;

namespace WebGui
{
    /// <summary>
    /// SomeSpecial 的摘要说明
    /// </summary>
    public class SomeSpecial : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            //context.Response.ContentType = "text/plain";
            //context.Response.Write("Hello World");
            string type = Prolink.Math.GetValueAsString(context.Request["action"]);
            string msg = string.Empty;
            switch (type)
            {
                case "copybase": msg=copybase(context); break;
                case "resetlayout": msg = ResetAdmimLayout(context); break;

            }
            HttpContext.Current.Response.Write(msg);
        }

        protected string copybase(HttpContext context)
        {
            string fromcmp = Prolink.Math.GetValueAsString(context.Request["fromcmp"]);
            string arrivecmp = Prolink.Math.GetValueAsString(context.Request["arrivecmp"]);
            string table = Prolink.Math.GetValueAsString(context.Request["basetable"]);
            if (arrivecmp != "")
            {
                fromcmp = fromcmp == "" ? "FQ" : fromcmp;

            }
            switch (table)
            {
                case "客户建档": table = "SMPTY"; break;
                case "客户交易建档": table = "SMSIM"; break;
                case "币别建档": table = "BSCUR"; break;
                case "汇率建档": table = "BSERATE"; break;
                case "代码建档": table = "BSCODE"; break;
                case "费用建档": table = "SMCHG"; break;
                case "过账节点建档": table = "SMPOST"; break;
            }
            string sql = string.Format("SELECT * FROM {0} WHERE CMP ={1}", table, SQLUtils.QuotedStr(arrivecmp));
            DataTable dt = null;
            try
            {
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception e)
            {
                return new JavaScriptSerializer().Serialize(new { message = e.Message, IsOk = "N" });
            }
            if (dt.Rows.Count > 0)
            {
                return new JavaScriptSerializer().Serialize(new { message = "该站已有建档，不允许从这里新增！", IsOk = "N" });
            }
            sql = string.Format("SELECT * FROM {0} WHERE CMP ={1}", table, SQLUtils.QuotedStr(fromcmp));
            MixedList ml = new MixedList();
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Func<Type, object, object> GetValue = (DataType, OValue) =>
            {
                switch (DataType.Name.ToLower())
                {
                    case "int": return Prolink.Math.GetValueAsInt(OValue);
                    case "decimal": return Prolink.Math.GetValueAsDecimal(OValue);
                    case "double":
                    case "numeric": return Prolink.Math.GetValueAsDouble(OValue);
                    case "datetime": return Prolink.Math.GetValueAsDateTime(OValue);
                    default: return Prolink.Math.GetValueAsString(OValue);
                }
            };
            foreach (DataRow dr in dt.Rows)
            {
                EditInstruct ei = new EditInstruct(table, EditInstruct.INSERT_OPERATION);
                foreach (DataColumn column in dt.Columns)
                {
                    string name = column.ColumnName;
                    if (name.Equals("CMP")) { ei.Put(name, arrivecmp); continue; }
                    if (name.Equals("CREATE_BY")) { ei.Put(name, "SYSTEM"); continue; }
                    if (name.Equals("CREATE_DATE")) { ei.PutExpress(name, "GETDATE()"); continue; }
                    if (name.Equals("MODIFY_BY") || name.Equals("MODIFY_DATE")) continue;
                    if (name.Equals("U_ID")) { ei.Put(name, System.Guid.NewGuid().ToString()); continue; }
                    if (dr[name] == System.DBNull.Value) continue;
                    var value = GetValue(column.DataType, dr[name]);
                    if (Prolink.Math.GetValueAsString(value) != "")
                        ei.Put(name, value);
                };
                ml.Add(ei);
            }
            if (ml.Count > 0)
            {
                string doc = string.Empty;
                for (int i = 0; i < ml.Count; i++)
                {
                    doc += OperationUtils.GetInsertSQL((EditInstruct)ml[i], table) + "\r\n";
                }
                BackupData(new List<string> { "SpecialImport", this.GetType().Name, "Document", table }, doc);
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception e)
                {
                    return new JavaScriptSerializer().Serialize(new { message = e.Message, IsOk = "N" });
                }
            }
            else
            {
                return new JavaScriptSerializer().Serialize(new { message = "没有("+fromcmp+")资料", IsOk = "N" });
            }
            return new JavaScriptSerializer().Serialize(new { message = "", IsOk = "Y" });
        }

        public string ResetAdmimLayout(HttpContext context)
        {
            string column = Prolink.Math.GetValueAsString(context.Request["column"]);
            string[] layoutId ={"FCL 訂艙", "LCL 訂艙",
                       "國際快遞訂艙", "國內快遞訂艙", "空運訂艙",
                       "內貿訂艙", "鐵路訂艙", "订舱列表"};
            string[] columns = column.Split(',');

            string condition = string.Empty;
            foreach (string str in layoutId)
            {
                if (!string.IsNullOrEmpty(condition)) condition += " OR ";
                condition += string.Format("ID LIKE '%{0}%'", str);
            }
            string sql = string.Format(@"SELECT * FROM SYS_LAYOUT WHERE ({0}) AND LAYOUT_TYPE='GRID' ", condition);
            DataTable layoutdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList ml = new MixedList();
            foreach (DataRow dr in layoutdt.Rows)
            {
                string layout = Prolink.Math.GetValueAsString(dr["LAYOUT"]);
                string groupid = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                string companyid = Prolink.Math.GetValueAsString(dr["CMP"]);
                string name = Prolink.Math.GetValueAsString(dr["ID"]);
                string layouttype = Prolink.Math.GetValueAsString(dr["LAYOUT_TYPE"]);
                foreach (string id in columns)
                {
                    if (layout.Contains(id)) continue;
                    layout = layout.Replace("]", ",{\"name\":\"" + id + "\",\"width\":100,\"hidden\":true}]");
                    EditInstruct ei1 = new EditInstruct("SYS_LAYOUT", EditInstruct.UPDATE_OPERATION);
                    ei1.PutKey("GROUP_ID", groupid);
                    ei1.PutKey("CMP", companyid);
                    ei1.PutKey("ID", name);
                    ei1.PutKey("U_ID", "ADMIN");
                    ei1.PutKey("LAYOUT_TYPE", layouttype);
                    ei1.Put("LAYOUT", layout);
                    ml.Add(ei1);
                }
            }
            if (ml.Count > 0)
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            else {
                return new JavaScriptSerializer().Serialize(new { message = "没有修改", IsOk = "N" });
            }
            return new JavaScriptSerializer().Serialize(new { message = "", IsOk = "Y" });
        }

        protected string BackupData(IEnumerable<string> dirPath, string doc, string fileName = "")
        {
            return BackupXml(dirPath, doc, fileName);
        }
        protected string BackupXml(IEnumerable<string> dirPath, string xml, string fileName = "")
        {
            if (string.IsNullOrEmpty(fileName)) fileName = GetCurrentTimeString();
            string backupFileName = CreateBaseDirectoryFileName(dirPath, fileName, "txt");
            BackupData(backupFileName, xml);
            return backupFileName;
        }
        protected string GetCurrentTimeString()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
        protected string CreateBaseDirectoryFileName(IEnumerable<string> modes, string fileName, string extension = "txt")
        {
            List<string> items = new List<string> { AppDomain.CurrentDomain.BaseDirectory, "../" };
            items.AddRange(modes);
            items.AddRange(new List<string> { DateTime.Now.ToString("yyyyMMdd"), string.Format("{0}.{1}", fileName, extension) });
            return Path.Combine(items.ToArray());
        }
        protected void BackupData(string filePath, string txt)
        {
            ExportToFile(filePath, txt);
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
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}