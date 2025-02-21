using Business.Service;
using Business.TPV.Base;
using Business.TPV.Standard;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using static TrackingEDI.InboundBusiness.ASNManager;

namespace Business.TPV.Import
{
    public class ASNDataManager : ManagerBase
    {
        public ResultInfo Import(string json)
        {
            ASNDataInfo info = new ASNDataInfo();
            JavaScriptSerializer js = new JavaScriptSerializer();
            ResultInfo result = new ResultInfo();
            try
            {
                info = js.Deserialize<ASNDataInfo>(json);
            }
            catch (Exception ex)
            {
                result = ResultInfo.UnknowResult("JSON格式异常");
                return result;
            }
            result = Save(info);
            EditInstructList el = new EditInstructList();
            ReceiveASNInfoEDILog log = new ReceiveASNInfoEDILog(info, "TaskSystem");
            var v = result.IsSucceed ? log.CreateSucceed(info.Invoice_Number, info) : log.CreateEx(result.Description, info.Invoice_Number, info);
            v.Data = json;
            el.Add(Helper.CreateEDIEi(v, el));

            try
            {
                DB.ExecuteUpdate(el);
            }
            catch
            {
            }

            return result;
        }

        private ResultInfo Save(ASNDataInfo info)
        {
            string sql = string.Format("SELECT U_ID FROM ASN_MAP WHERE CNTR_NO={0}", SQLUtils.QuotedStr(info.Invoice_Number));
            string uid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("ASN_MAP", EditInstruct.INSERT_OPERATION);
            if(!string.IsNullOrEmpty(uid))
            {
                ei = new EditInstruct("ASN_MAP", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uid);
                ei.PutDate("MODIFY_DATE", DateTime.Now);
            }
            else
            {
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.PutDate("CREATE_DATE", DateTime.Now);
            }
            ei.Put("GROUP_ID", "TPV");
            ei.Put("CNTR_NO", info.Invoice_Number);
            ei.Put("ASN_NO", info.ASN_Number);
            ei.PutDate("ASN_DATE", info.ASN_Date);
            ei.PutDate("SEND_TIME", info.Send_Time);
            ei.PutDate("GR_DATE", info.GR_Date);
            ei.Put("GR_STATUS", info.Gr_Status);
            ei.Put("ASN_Date_Update", info.ASN_Date_Update);
            ei.Put("CHANGED_BY", info.ChangedBy);
            ei.Put("RESULT_STATUS", "N");
            ml.Add(ei);

            sql = string.Format(@"SELECT U_ID,SHIPMENT_ID,INV_NO,ASN_DATE,
(SELECT TOP 1 ETA FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS ETA,
(SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS TRAN_TYPE,
(SELECT TOP 1 CMP FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS CMP 
            FROM SMIDN WHERE INV_NO={0}",
                SQLUtils.QuotedStr(info.Invoice_Number));
            DataTable smidnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> ShipmentList = new List<string>();

            foreach (DataRow dr in smidnDt.Rows)
            {
                string inUid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                if (!string.IsNullOrEmpty(shipmentId) && !ShipmentList.Contains(shipmentId))
                    ShipmentList.Add(shipmentId);
                EditInstruct smidnei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                smidnei.PutKey("U_ID", inUid);
                smidnei.Put("ASN_NO", info.ASN_Number);
                if (info.ASN_Date.CompareTo(new DateTime(2000, 1, 1)) > 0)
                {
                    DateTime asnDatetemp = info.ASN_Date;
                    if(!(info.ASN_Date_Update == "Y" && info.ChangedBy != "ESP_RFC") && !string.IsNullOrEmpty(shipmentId))
                    {
                        TrackingEDI.InboundBusiness.ASNManager.Datetype datetype = TrackingEDI.InboundBusiness.ASNManager.Getasndate(shipmentId, info.Invoice_Number, dr, ref asnDatetemp);
                        switch (datetype)
                        {
                            case Datetype.DeliveryDate:
                            case Datetype.ETA:
                                smidnei.Put("SEND_ASN_STATUS", "N");
                                break;
                            case Datetype.MMNUpdate:
                                smidnei.Put("SEND_ASN_STATUS", "F");
                                break;
                        }
                    }

                    smidnei.PutDate("ASN_DATE", asnDatetemp);
                    smidnei.PutDate("GR_DATE", info.GR_Date);
                    smidnei.Put("GR_STATUS", info.Gr_Status);
                }

                ml.Add(smidnei);
            }

            return Execute(ml);
        }
    }
}
