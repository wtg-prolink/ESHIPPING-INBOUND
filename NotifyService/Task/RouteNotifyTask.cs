using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Prolink.Data;
using Prolink.Task;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using TrackingEDI.Utils;

namespace NotifyService.Task
{
    public class RouteNotifyTask : IPlanTask
    {
        public void Run(IPlanTaskMessenger messenger)
        {
            RouteNotify.Run();
        }
    }
}
