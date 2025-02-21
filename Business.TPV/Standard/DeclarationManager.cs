using Business.Service;
using Business.TPV.Base;
using Business.TPV.Utils;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.Standard
{
    public class DeclarationManager : ImprotManager<DeclarationInfo>
    {
        public override ResultInfo ImportInstanceList(IEnumerable<DeclarationInfo> infos)
        {
            var result = base.ImportInstanceList(infos);
            foreach (var item in infos)
            {
                OceanDeclarationResponedEDILog log = new OceanDeclarationResponedEDILog(item);
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

        protected override ResultInfo HandlerFile(DeclarationInfo info, DataRow smRow)
        {
            ResultInfo r = UnknowResult();
            if (info.Items == null || info.Items.Length <= 0) return r;
            foreach (var item in info.Items)
            {
                string filePath = SaveFile(item.FileData, item.FileExtension, item.DNNO);
                r = UploadFile(item.DNNO, smRow, filePath);
                if (!r.IsSucceed)
                {
                    r.IsSucceed = false;
                    r.ResultCode = "SAVEFILE_ERROR";
                    r.Description = string.Format("{0}{1}{2}", r.Description, Environment.NewLine, r.Description);
                }
            }
            return r;
        }

        protected override EditInstructList ToEi(DeclarationInfo obj)
        {
            EditInstructList eiList = new EditInstructList();
            eiList.MergeEditInstruct(Helper.CreateDeclarationUpEi(obj));
            eiList.MergeEditInstructList(CteateUpDNEiList(obj));
            return eiList;
        }

        EditInstructList CteateUpDNEiList(DeclarationInfo obj)
        {
            if (obj.Items == null || obj.Items.Length <= 0) return null;
            EditInstructList eiList = new EditInstructList();
            foreach (var item in obj.Items)
            {
                EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                ei.Condition = string.Format("ORIGIN_NO={0} AND SHIPMENT_ID={1}", SQLUtils.QuotedStr(item.DNNO), SQLUtils.QuotedStr(obj.ShipmentID));
                ei.Put("APPROVE_NO", item.ApprovalNumber);
                ei.Put("EDECL_NO", item.DeclarationNumber);
                ei.Put("NEXT_NUM", item.NextNum);
                if (item.DeclarationDate.HasValue)
                    ei.PutDate("DECL_DATE", item.DeclarationDate.Value);
                if (item.DeclarationReleaseDate.HasValue)
                    ei.PutDate("DECL_RLS_DATE", item.DeclarationReleaseDate.Value);
                eiList.Add(ei);
            }
            return eiList;
        }

        protected override ResultInfo HandlerAdd(List<DeclarationInfo> infos)
        {
            var r = base.HandlerAdd(infos);
            if (r != null && r.IsSucceed)
            {
                foreach (var v in infos)
                {
                    TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status
                    {
                        ShipmentId = v.ShipmentID,
                        StsCd = "200",
                        Sender = v.Sender,
                        StsDescp = "Customs declaration to confirm"
                    });
                }
            }
            return r;
        }

        protected override ResultInfo HandlerCancel(DeclarationInfo info)
        {
            EditInstruct ei = Helper.CreateCancelDeclaration(info);
            return Execute(ei);
        }

        protected override ResultInfo CheckPartyInfo(DeclarationInfo info)
        {
            var bkNO = Helper.GetBroker(info.ShipmentID);
            if (string.IsNullOrEmpty(bkNO))
            {
                return new ResultInfo
                {
                    ResultCode = "BrokerIsNull",
                    Description = "验证该笔报关代理异常,未找到报关代理!"
                };
            }
            if (bkNO == info.Sender) return SucceedResult();
            return new ResultInfo
            {
                ResultCode = "BrokerError",
                Description = "验证该笔报关确认不通过,报关代理不匹配，请验证节点Sender值是否有误!"
            };
        }
    }
}
