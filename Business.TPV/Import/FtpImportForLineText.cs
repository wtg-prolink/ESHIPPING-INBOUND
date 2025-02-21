using Business.Import;
using Business.Utils;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.Import
{
    abstract class TpvFtpImportForLineText : FtpImportForLineText, IPlanTask
    {
        protected abstract string FileName { get; }

        protected override string ConfigFileName
        {
            get
            {
                return System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, string.Format("edi/ftp/{0}.xml", FileName));
            }
        }

        public void Run(IPlanTaskMessenger messenger)
        {
            this.AnalyzeFile();
        }
    }
}
