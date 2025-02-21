using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Import.DNImport
{
    class Import:FtpImportBase
    {
        public void Do()
        {

        }

        protected override bool OperateFile(string filePath)
        {
            throw new NotImplementedException();
        }

        protected override Utils.FTPConfig GetFtpConfig()
        {
            throw new NotImplementedException();
        }
    }
}
