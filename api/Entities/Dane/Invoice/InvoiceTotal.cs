namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceTotal
    {
        public int InvoiceTotalId { get; set; }
        public double TotalBrutto { get; set; }
        public double TotalNetto { get; set; }
        public double TotalTax { get; set; }

        
        public InvoiceBuy InvoiceBuy { get; set; }
        public int? InvoiceBuyId { get; set; }

        public InvoiceSell InvoiceSell { get; set; }
        public int? InvoiceSellId { get; set; }

    }
}