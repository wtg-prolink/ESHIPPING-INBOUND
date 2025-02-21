using Business.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.Import
{
    public abstract class FtpImportForXml : FtpImportBaseForConfig
    {
        protected abstract bool OperateFile(XmlDocument doc);

        protected override bool OperateFile(FtpImportEvertArgs args)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(args.LocalFileName);
                return OperateFile(doc);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(string.Format("解析文档出现异常!:{0}", args.LocalFileName));
                throw ex;
            }
        }
    }

    public class XmlTemplate
    {

    }
}