using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrackingEDI.Business;

namespace NotifyService.Task
{
    public class TrackingEvenTask : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;

            string[]evenTypes = EvenFactory.GetEvenType();

            foreach (string et in evenTypes)
            {
                EvenFactory.ExecuteEven(et);
            }

        }

        private void Test()
        {
            EvenFactory.RegisterEvenTask(EvenManager.StatusEven, EvenManager.StatusNotify);
            EvenFactory.RegisterEvenTask(EvenManager.StatusEven1, EvenManager.StatusNotify1);
            EvenFactory.RegisterEvenTask(EvenManager.TrackingEven, EvenManager.TrackingNotify);

            string u_id = "3c3ff2c856b74d1496e8553454671d5e";
            string seq_no = "0001e5e79b4841a0b82c93902a0aa5d2";
            string code = "S00";
            EvenFactory.AddOnceEven(u_id, u_id, EvenManager.StatusEven1);
            EvenFactory.AddOnceEven(u_id, u_id, EvenManager.TrackingEven);
            EvenFactory.AddOnceEven(string.Format("{0}#{1}#{2}", u_id, seq_no, code), u_id, EvenManager.StatusEven);
            string[] evenTypes = EvenFactory.GetEvenType();
            foreach (string et in evenTypes)
            {
                EvenFactory.ExecuteEven(et);
            }
        }
    }
}