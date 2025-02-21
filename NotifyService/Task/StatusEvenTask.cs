using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prolink.Task;
using TrackingEDI.Business;

namespace NotifyService.Task
{
    public class StatusEvenTask : IPlanTask
    {
        public void Run(IPlanTaskMessenger messenger)
        {
            //Prolink.EDOC_API _api = new Prolink.EDOC_API();
            //_api.Login();
            //string guid = _api.GetFolderGUID("123", "*", "SD", "FQ", "");
            //List<EDOCFileItem> edocList = _api.Inquery(guid);
            //_api.DownloadFile(@"d:/xx.dada", "3", "");
            EvenFactory.ExecuteStatusEven("ST");
        }
    }
}
