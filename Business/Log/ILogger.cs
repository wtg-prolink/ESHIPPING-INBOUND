using Prolink.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Business.Log
{
    public interface ILogger
    {
        void WriteLog(LogInfo info);
    }

    public class DLogger : ManagerBase, ILogger
    {
        public virtual void WriteLog(LogInfo info)
        {
            if (info == null) return;
            try
            {
                DocLogger.WriteLog(ToJsonString(info));
            }
            catch
            {

            }
        }

        public virtual void WriteLog(Exception ex)
        {
            WriteLog(ex.ToString());
        }

        protected string ToJsonString(object obj)
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            return Serializer.Serialize(obj);
        }

        DefaultLogger _logger;
        DefaultLogger DocLogger
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

        public void WriteLog(string msgInfo)
        {
            WriteLog(msgInfo, (string)null);
        }
        public void WriteLog(string msgInfo, Exception ex)
        {
            WriteLog(msgInfo, null, null, null, ex == null ? null : ex.ToString());
        }
        public void WriteLog(string msgInfo, string msgType)
        {
            WriteLog(msgInfo, msgType, null);
        }
        public void WriteLog(string msgInfo, string msgType, string refNO)
        {
            WriteLog(msgInfo, msgType, refNO, null);
        }
        public void WriteLog(string msgInfo, string msgType, string refNO, string createBy)
        {
            WriteLog(msgInfo, msgType, refNO, createBy, null);
        }
        public void WriteLog(string msgInfo, string msgType, string refNO, string createBy, string remark)
        {
            WriteLog(CreateLog(msgInfo, msgType, refNO, createBy, remark));
        }

        public LogInfo CreateLog(string msgInfo)
        {
            return CreateLog(msgInfo, null);
        }
        public LogInfo CreateLog(string msgInfo, string msgType)
        {
            return CreateLog(msgInfo, msgType, null);
        }
        public LogInfo CreateLog(string msgInfo, string msgType, string refNO)
        {
            return CreateLog(msgInfo, msgType, refNO, null);
        }
        public LogInfo CreateLog(string msgInfo, string msgType, string refNO, string createBy)
        {
            return CreateLog(msgInfo, msgType, refNO, createBy, null);
        }
        public virtual LogInfo CreateLog(string msgInfo, string msgType, string refNO, string createBy, string remark)
        {
            return new LogInfo
            {
                ID = System.Guid.NewGuid().ToString(),
                CreateBy = createBy,
                MsgInfo = msgInfo,
                MsgType = msgType,
                RefNO = refNO,
                Remark = remark
            };
        }
    }
}