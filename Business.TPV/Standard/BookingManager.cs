using Business.Service;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Standard
{
    public abstract class BookingManager<T, TRuntime> : ImprotManager<T>
        where T : BookingResponse
        where TRuntime : BookingRuntime<T>,new()
    {

        protected override ResultInfo HandlerCancel(T info)
        {
            Helper.CancelBooking(info.ShipmentID, info.Remark);
            return SucceedResult();
        }

        protected override ResultInfo HandlerAdd(List<T> infos)
        {
            EditInstructList eiList = new EditInstructList();
            dynamic runtimeObj = BeforeCreateRuntime(infos);
            foreach (var item in infos)
            {
                BackupData(item, string.Format("{0}_{1}", item.ShipmentID, GetCurrentTimeString()));
                TRuntime runtime = CreateRuntime(item, runtimeObj);
                eiList.Add(CreateEi(runtime));
            }
            CheckEiList(eiList);
            ResultInfo resultInfo = Execute(eiList);
            try
            {
                if (resultInfo.IsSucceed)
                {
                    foreach (var v in infos)
                    {
                        var result = Helper.ConfirmBooking(v.ShipmentID, v.Sender);
                        if (!result.IsSucceed) return new ResultInfo { Description = result.Description, ResultCode = result.ResultCode };
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("订舱确认异常!", ex);
            }
            return resultInfo;
        }

        protected virtual dynamic BeforeCreateRuntime(List<T> infos)
        {
            return null;
        }

        protected abstract TRuntime CreateRuntime(T info, dynamic runtimeObj);

        void CheckEiList(EditInstructList eiList)
        {
            for (int i = 0; i < eiList.Count; i++)
            {
                EditInstruct ei = eiList[i];
                string[] names = ei.GetDateSet();
                foreach (var name in names)
                {
                    string val = ei.Get(name);
                    if (string.IsNullOrEmpty(val)) continue;
                    DateTime time = ParseToDateTime(val, "yyyyMMddHHmmss");
                    if (time == DateTime.MinValue)
                        ei.Remove(name);
                }
            }
        }

        protected override EditInstructList ToEi(T obj)
        {
            return null;
        }

        protected virtual EditInstruct CreateEi(TRuntime runtime)
        {
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(runtime.Info.ShipmentID));
            return ei;
        }

        protected DateTime? ParseToDateTimeForNullValue(string dateStr, string formart = "yyyyMMddHHmm")
        {
            DateTime dt = ParseToDateTime(dateStr, formart);
            if (dt == DateTime.MinValue) return null;
            return dt;
        }

        protected DateTime ParseToDateTime(string dateStr, string formart = "yyyyMMddHHmm")
        {
            if (string.IsNullOrEmpty(dateStr)) return DateTime.MinValue;
            DateTime time;
            if (DateTime.TryParseExact(dateStr, formart, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat,
                System.Globalization.DateTimeStyles.None, out time))
            {
                return time;
            }
            else
            {
                if (DateTime.TryParse(dateStr, out time))
                    return time;
            }
            return DateTime.MinValue;
        }
    }

    public class BookingRuntime<T> where T : BookingResponse
    {
        public T Info { get; set; }
    }
}

