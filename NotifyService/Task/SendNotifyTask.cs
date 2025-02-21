using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prolink.Task;
using TrackingEDI.Business;


namespace NotifyService.Task
{
    /// <summary>
    /// 发送货况通知
    /// </summary>
    public class SendNotifyTask : IPlanTask
    {
        public void Run(IPlanTaskMessenger messenger)
        {
           
            EvenNotify.SendNotify("NOTIFY_CD");
            //EvenNotify.SendNotify("REQUEST_CD");
        }
    }
}
