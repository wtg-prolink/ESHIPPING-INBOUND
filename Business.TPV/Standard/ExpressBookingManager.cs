using Business.Service;
using Business.TPV.Base;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.Standard
{
    public class ExpressBookingManager : BookingManager<ExpressBookingResponse, BookingRuntime<ExpressBookingResponse>>
    {
        public override ResultInfo ImportInstanceList(IEnumerable<ExpressBookingResponse> infos)
        {
            var result = base.ImportInstanceList(infos);
            foreach (var item in infos)
            {
                ExpressBookingResponedEDILog log = new ExpressBookingResponedEDILog(item);
                if (result.IsSucceed)
                {
                    Logger.WriteLog(log.CreateSucceed());
                }
                else
                {
                    Logger.WriteLog(log.CreateEx(result.Description));
                }
            }
            return result;
        }

        protected override EditInstruct CreateEi(BookingRuntime<ExpressBookingResponse> runtime)
        {
            EditInstruct ei = base.CreateEi(runtime);
            ei.Put("HOUSE_NO", runtime.Info.TackingNumber);
            ei.Put("MASTER_NO", runtime.Info.TackingNumber);
            ei.Put("DRIVER", runtime.Info.Driver);
            ei.Put("DRIVER_TEL", runtime.Info.DriverPhone);
            ei.Put("TRUCK_NO", runtime.Info.CarNubmer);
            if (runtime.Info.ETD.HasValue)
                ei.PutDate("ETD", runtime.Info.ETD.Value);
            if (runtime.Info.ETA.HasValue)
                ei.PutDate("ETA", runtime.Info.ETA.Value);
            return ei;
        }
        protected override BookingRuntime<ExpressBookingResponse> CreateRuntime(ExpressBookingResponse info, dynamic runtimeObj)
        {
            return new BookingRuntime<ExpressBookingResponse> { Info = info };
        }
    }
}
