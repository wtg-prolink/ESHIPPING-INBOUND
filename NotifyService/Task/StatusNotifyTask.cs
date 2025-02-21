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
    /// 设置货况通知启动时间
    /// </summary>
    public class StatusNotifyTask : IPlanTask
    {
        public void Run(IPlanTaskMessenger messenger)
        {
            EvenNotify.StatusNotify("NOTIFY_CD");
            //EvenNotify.StatusNotify("REQUEST_CD");
        }
    }
}
