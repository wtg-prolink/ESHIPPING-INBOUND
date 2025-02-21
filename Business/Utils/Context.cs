using Business.Log;
using Business.Service;
using Prolink.Persistence;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Business.Utils
{
    public class Context
    {
        private static string _runtimePath;
        public static string RuntimePath
        {
            get
            {
                if (string.IsNullOrEmpty(_runtimePath))
                {
                    string fileName = typeof(Context).Assembly.Location;
                    _runtimePath = System.IO.Path.GetDirectoryName(fileName);
                }
                return _runtimePath;
            }
        }

        /// <summary>
        ///  获取DB的类别
        /// </summary>
        /// <param name="name">为空抓取默认DB</param>
        /// <returns></returns>
        public static DBModes GetDBModes(string databaseCode = "")
        {
            Database db = null;
            if (string.IsNullOrEmpty(databaseCode))
                db = DatabaseFactory.GetDefaultDatabase();
            else
                db = DatabaseFactory.GetDatabase(databaseCode);
            if (db == null) throw new Exception("Database not initialization!");
            string type = db.PoolType.ToUpper();
            string sql = string.Empty;
            switch (type)
            {
                case "ORACLE": return DBModes.Oracle;
                default: return DBModes.SqlServer;
            }
        }

        private static string _xmlStorePath;
        public static string XmlStorePath
        {
            get
            {
                if (string.IsNullOrEmpty(_xmlStorePath))
                {
                    Prolink.Web.WebContext webContext = Prolink.Web.WebContext.GetInstance();
                    if (webContext != null)
                    {
                        string path = Prolink.Web.WebContext.GetInstance().GetAppPath();
                        _xmlStorePath = Path.Combine(path, "../doc/xml-store/");
                    }
                    else
                    {
                        _xmlStorePath = Path.Combine(RuntimePath, "../../../doc/xml-store/");
                    }
                }
                return _xmlStorePath;
            }
            set
            {
                _xmlStorePath = value;
            }
        }

        public static ISecurity SecurityHandler { get; set; }

        private static DLogger _logger;
        public static DLogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = new DLogger();
                }
                return _logger;
            }
            set
            {
                _logger = value;
            }
        }
    }

    public enum DBModes{SqlServer,Oracle}
}
