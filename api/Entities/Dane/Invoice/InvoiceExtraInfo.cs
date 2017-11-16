using System;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceExtraInfo
    {
        public int InvoiceExtraInfoId { get; set; }

        public string CmrName { get; set; }
        public bool CmrRecived { get; set; }
        public DateTime? CmrRecivedDate { get; set; }

        public bool InvoiceSent { get; set; }
        public string InvoiceSentNo { get; set; }
        public DateTime? InvoiceRecivedDate { get; set; }

        public string LoadNo { get; set; }
        public string TaxExchangedInfo { get; set; }

        public InvoiceSell InvoiceSell { get; set; }
        public int InvoiceSellId { get; set; }
    }
}