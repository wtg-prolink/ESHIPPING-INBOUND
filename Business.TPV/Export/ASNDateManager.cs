using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
namespace Business.TPV.Export
{
    public class ASNDateManager : ManagerBase
    {
        public ResultInfo TryPostASNDateInfo(ASNDateInfo item, string location="")
        {
            try
            {
                if (item == null)
                {
                    return ResultInfo.NullDataResult();
                }

                ASNDateEDI edi = new ASNDateEDI();

                EditInstructList el = new EditInstructList();
                var result = edi.TryPostASNDateInfo(item, location);
                PostASNDateInfoEDILog log = new PostASNDateInfoEDILog(item, "TaskSystem");
                var v = result.IsSucceed ? log.CreateSucceed(item.InvNo, item) : log.CreateEx(result.Description, item.InvNo, item);
                v.Data = ToJsonString(item);
                el.Add(Helper.CreateEDIEi(v, el));
                if (el != null && el.Count > 0)
                {
                    Execute(el);
                }
                return result;
            }
            catch (Exception ex)
            {
                PostASNDateInfoEDILog log = new PostASNDateInfoEDILog(item, "TaskSystem");
                string str = ex.ToString();
                if (str.Length > 500)
                {
                    str= str.Substring(0, 500);
                }
                EditInstructList el = new EditInstructList();
                var v = log.CreateEx(str, item.InvNo, item);
                v.Data = ToJsonString(item);
                el.Add(Helper.CreateEDIEi(v, el));
                if (el != null && el.Count > 0)
                {
                    Execute(el);
                }
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("SendASNDateTask Error:" + ex.Message);
                return UnknowResult(ex);
            }
        }

        public void DsiposeDestination()
        {
            ASNDateEDI edi = new ASNDateEDI();
             edi.DsiposeDestination();
        }
    }
}
