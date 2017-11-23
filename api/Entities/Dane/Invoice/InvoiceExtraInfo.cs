using System;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceExtraInfo
    {
        public int InvoiceExtraInfoId { get; set; }

        public InvoiceExtraInfoChecked Cmr { get; set; }
        public InvoiceExtraInfoChecked Recived { get; set; }
        public InvoiceExtraInfoChecked Sent { get; set; }

        public string LoadNo { get; set; }
        public string TaxExchangedInfo { get; set; }

        public InvoiceSell InvoiceSell { get; set; }
        public int InvoiceSellId { get; set; }
    }
}