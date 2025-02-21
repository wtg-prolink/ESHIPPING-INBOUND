using Business.TPV.Base;
using Business.TPV.RFC;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.Import
{
    /// <summary>
    /// 发货过账
    /// </summary>
    public class DeliveryPostingManager : ManagerBase
    {
        public bool TryPostingDate(string sapId, DeliveryPostingInfo info, out DPResultInfo result, string location)
        {
            Prolink.Persistence.Database db = DB;
            bool ispost = false;
            string exceptionstr = string.Empty;
            try
            {
                RFC.DeliveryPostingEDI edi = new DeliveryPostingEDI();
                string fileName = BackupData(info, new List<string> { "Export", "Delivery" }, string.Format("{0}_{1}", info.DNNO, GetCurrentTimeString()));
                ispost = edi.TryPostDeliveryPostingInfo(sapId, info, out result, location);
                PostBillInfoEDILog log = new PostBillInfoEDILog(info, "TaskSystem");
                var v = result.MsgType == "S" ? log.CreateSucceed(info.DNNO, info) : log.CreateEx(result.MsgText, info.DNNO, info);
                EditInstructList el = new EditInstructList();
                v.Data = ToJsonString(info);
                el.Add(Helper.CreateEDIEi(v, el));
                if (el != null && el.Count > 0)
                {
                    Execute(el);
                }
                return ispost;
            }
            catch (Exception ex)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("TryPostingDate", ex);
                exceptionstr = ex.Message.ToString();
            }
            result = null;
            if (!string.IsNullOrEmpty(exceptionstr))
            {
                result = new DPResultInfo();
                result.MsgText = exceptionstr;
            }
            return ispost;
        }

        public void DsiposeDestination()
        {
            RFC.DeliveryPostingEDI edi = new RFC.DeliveryPostingEDI();
            edi.DsiposeDestination();
        }
    }

    public class DeliveryPostingInfo
    {
        public string DNNO { get; set; }
        public string CMP { get; set; }
        /// <summary>
        /// 过账日期
        /// </summary>
        public DateTime? GoodsMovementDate { get; set; }
    }
}
