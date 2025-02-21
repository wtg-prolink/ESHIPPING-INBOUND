using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prolink.Task;
using TrackingEDI.Business;

namespace NotifyService.Task
{
    public class SendMailTask : IPlanTask
    {
        public void Run(IPlanTaskMessenger messenger)
        {
            string []mailTypes = EvenFactory.GetMailType();
            foreach (string mt in mailTypes)
            {
                EvenFactory.ExecuteMailEven(mt);//route notify的通知
            }
        }
    }
}
