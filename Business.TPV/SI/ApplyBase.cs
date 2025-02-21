using Models.EDI;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.SI
{
    public class ApplyBase
    {
        public void FillInvoiceEi(SIProfileInfo pInfo, EditInstruct invEi)
        {
            if (!string.IsNullOrEmpty(pInfo.ShippingMark))
                invEi.Put("MARKS", pInfo.ShippingMark);
            if (!string.IsNullOrEmpty(pInfo.InvoiceRemark))
                invEi.Put("INVOICE_RMK", pInfo.InvoiceRemark);
            if (!string.IsNullOrEmpty(pInfo.PackingRemark))
                invEi.Put("PACKING_RMK", pInfo.PackingRemark);
            if (!string.IsNullOrEmpty(pInfo.InvoiceIntruction))
                invEi.Put("INV_INTRU", pInfo.InvoiceIntruction);
            if (!string.IsNullOrEmpty(pInfo.PackingIntruction))
                invEi.Put("PKG_INTRU", pInfo.PackingIntruction);
        }
    }
}