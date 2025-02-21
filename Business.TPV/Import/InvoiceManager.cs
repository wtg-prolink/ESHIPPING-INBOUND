using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.Import
{
    class InvoiceManager : ManagerBase
    {
        public ResultInfo Import(DNInfo info,Func<string,string,string>  getOtherValue, OperationModes mode= OperationModes.Add)
        {
            return null;
        }

        EditInstructList ToEiList(DNInfo info, Func<string, string, string> getOtherValue, OperationModes mode)
        {
            EditInstructList eiList = new EditInstructList();
            string invID = string.Empty;
            EditInstruct mainEi = ToInvM(info, getOtherValue, mode, out invID);
            eiList.Add(mainEi);
            EditInstructList detailInvEiList = ToInvDetail(info, getOtherValue,mode, invID);
            eiList.MergeEditInstructList(detailInvEiList);
            UpMainTotalForInvDetail(mainEi, detailInvEiList);
            EditInstructList packingEiList = ToPacking(info, mode);
            eiList.MergeEditInstructList(packingEiList);
            UpMainTotalForPacking(mainEi,packingEiList);
            return eiList;
        }

        void UpMainTotalForInvDetail(EditInstruct mainEi, EditInstructList detailEiList)
        {
            Func<List<InvDetailNumericalColumnValues>> getItems = () =>
            {
                return new List<InvDetailNumericalColumnValues>();
            };
            List<InvDetailNumericalColumnValues> items = getItems();
            double amount = 0;
            items.ForEach(item => amount += item.Amount);
            mainEi.Put("TTL_VALUE", amount);
        }

        void UpMainTotalForPacking(EditInstruct mainEi, EditInstructList detailEiList)
        {
            Func<List<PackingNumericalColumnValues>> getItems = () =>
            {
                return new List<PackingNumericalColumnValues>();
            };
            List<PackingNumericalColumnValues> items = getItems();
            double amount = 0;
            items.ForEach(item => amount += 0);
            mainEi.Put("TTL_VALUE", amount);
        }
        class PackingNumericalColumnValues
        {

        }

        class InvDetailNumericalColumnValues
        {
            public double Amount { get; set; }
        }

        EditInstruct ToInvM(DNInfo info,Func<string, string, string> getOtherValue, OperationModes mode,out string id)
        {
            id=null;
            EditInstruct ei = null;
            switch (mode)
            {
                case OperationModes.Add:
                    ei = new EditInstruct("SMINM", EditInstruct.INSERT_OPERATION);
                    id=System.Guid.NewGuid().ToString();
                     ei.Put("U_ID",id);
                    ei.PutDate("CREATE_DATE",DateTime.Now);

                    break;
                case OperationModes.Modify:
                    ei = new EditInstruct("SMINM", EditInstruct.INSERT_OPERATION);
                    ei.Condition=string.Format("INV_NO={0}",SQLUtils.QuotedStr(info.HeaderInfo.DNNO));
                    break;
            }
            ei.Put("GROUP_ID",Context.GroupId);
            ei.Put("CMP",info.HeaderInfo.CompanyCode);
            ei.Put("INVOICE_TYPE","O");
            ei.PutDate("INV_DATE",DateTime.Now);
            //ei.Put("SHIPMENT_ID",);
            ei.Put("DN_NO",info.HeaderInfo.DNNO);
            ei.Put("INV_NO",info.HeaderInfo.DNNO);
            //ei.Put("PACKING_NO",info.);
            //ei.Put("PO_NO",info.);
            //ei.Put("SO_NO",info.);
            //ei.Put("VAT_NO",info.);
            //ei.Put("REF_NO",info.);
            //ei.Put("ETD",info.);
            //ei.Put("ETA",info.);
            //ei.Put("VESSEL",info.);
            //ei.Put("SHIPPING_DATE",info.);
            //ei.Put("BL_NO",info.);
            ei.Put("DLV_WAY",info.HeaderInfo.ShippingConditions);
            //ei.Put("SHPR_CE",info.);
            //ei.Put("SHPR_NM",info.);
            //ei.Put("SHPR_ADDR",info.);
            //ei.Put("BILL_TO",info.);
            //ei.Put("BILL_NM",info.);
            //ei.Put("SHIP_TO",info.);
            //ei.Put("SHIP_NM",info.);
            //ei.Put("CNEE_CD",info.);
            //ei.Put("CNEE_NM",info.);
            //ei.Put("NOTIFY_NO",info.);
            //ei.Put("NOTIFY_NM",info.);
            ei.Put("CMDTY_CD",info.HeaderInfo.MeansOfTransportType);
            ei.Put("CMDTY_DESCP",info.HeaderInfo.MeansOfTransportTypeDescription);
            ei.Put("PAY_TERM_CD",info.HeaderInfo.TermsOfPaymentKey);
            ei.Put("PAY_DESCP",info.HeaderInfo.TermsOfPaymentKeyDescription);
            string incotermsDesc=getOtherValue(DNManager.IncotermsDescValueCode,info.HeaderInfo.Incoterms1);
            ei.Put("INCORTER",info.HeaderInfo.Incoterms1);
            ei.Put("INCORTER_DESCP",incotermsDesc);
            //ei.Put("FROM_CD",info.);
            //ei.Put("FROM_DESCP",info.);
           // ei.Put("TO_CD",info.);
           // ei.Put("TO_DESCP",info.);
            //ei.Put("CNTRY_ORN",info.);
            //ei.Put("CNTRY_DESCP",info.);
            //ei.Put("CUR",info.HeaderInfo.Currency);
            //ei.Put("TTL_PLT",info.);
            //ei.Put("PLTU",info.);
            //ei.Put("TTL_QTY",info.);
            //ei.Put("QTYU",info.);
            //ei.Put("TTL_NM",info.);
            //ei.Put("NWU",info.);
            //ei.Put("TTL_GW",info.);
            //ei.Put("TTL_CBM",info.);
            //ei.Put("MARKS",info.);
            //ei.Put("INVOICE_RMK",info.);
            //ei.Put("PACKING_RMK",info.);
            //ei.Put("COST",info.);
            //ei.Put("CREATE_BY",info.);
            //ei.Put("CREATE_DATE",info.);
            //ei.Put("CREATE_EXT",info.);
            //ei.Put("MODIFY_BY",info.);
            //ei.Put("MODIFY_DATE",info.);
            return ei;
        }

        EditInstructList ToInvDetail(DNInfo info, Func<string, string, string> getOtherValue, OperationModes mode, string invID)
        {
            if (info.ItemInfos == null || info.ItemInfos.Length <= 0) return null;
            EditInstructList eiList = new EditInstructList();
            foreach (var item in info.ItemInfos)
            {
                EditInstruct ei = null;
                switch (mode)
                {
                    case OperationModes.Add:
                        ei = new EditInstruct("SMIND", EditInstruct.INSERT_OPERATION);
                        ei.Put("U_ID", System.Guid.NewGuid().ToString());
                        ei.Put("U_FID", invID);
                        break;
                    case OperationModes.Modify:
                        ei = new EditInstruct("SMIND", EditInstruct.UPDATE_OPERATION);
                        break;
                }
                //ei.Put("SHIPMENT_ID",item.);
                ei.Put("DN_NO", item.DNNO);
                ei.Put("INV_NO", item.DNNO);
                //ei.Put("SEQ_NO",item.S);
                ei.Put("PO_NO", item.CustomerPurchaseOrderNumber);
                ei.Put("SO_NO", item.DNNO);
                ei.Put("OPART_NO", item.CustomerModelName);
                ei.Put("PART_NO", item.MaterialBelonging);
                ei.Put("IPART_NO", item.MaterialNumber);
                ei.Put("IHS_CODE", item.DestinationCountriesCommodityCode);
                ei.Put("GOODS_DESCP", item.ShortText);
                string brand = getOtherValue(DNManager.BrandDescValueCode, item.DeliveryItem);
                ei.Put("BRAND", brand);
                ei.Put("QTY", item.ActualQuantityDelivered);
                ei.Put("QTYU", item.SalesUnit);
                string cur1 = getOtherValue(DNManager.Currency1ValueCode, item.DeliveryItem);
                ei.Put("CUR1", cur1);
                string curUnit1 = getOtherValue(DNManager.CurrencyUnit1ValueCode, item.DeliveryItem);
                ei.Put("UNIT_PRICE1", curUnit1);
                double amount = Prolink.Math.GetValueAsDouble(item.ActualQuantityDelivered) *
                    Prolink.Math.GetValueAsDouble(curUnit1);
                ei.Put("AMT", amount);
                ei.Put("REMARK", item.ShortText);
                eiList.Add(ei);
            }
            return eiList;
        }

        EditInstructList ToPacking(DNInfo info, OperationModes mode)
        {
            return null;
        }
    }

    class DescriptionItem
    {
        public string MemberName { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }

    enum OperationModes { Add, Modify }
}
