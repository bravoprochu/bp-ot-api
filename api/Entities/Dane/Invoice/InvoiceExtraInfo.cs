namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceExtraInfo
    {
        public int InvoiceExtraInfoId { get; set; }
        public string LoadNo { get; set; }
        public string TaxExchangedInfo { get; set; }

        public InvoiceSell InvoiceSell { get; set; }
        public int InvoiceSellId { get; set; }
    }
}