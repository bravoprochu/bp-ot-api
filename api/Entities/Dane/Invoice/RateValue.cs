namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class RateValue
    {
        public int RateValueId { get; set; }
        public double BruttoValue { get; set; }
        public InvoiceSell InvoiceSell { get; set; }
        public InvoiceBuy InvoiceBuy { get; set; }
        public double NettoValue { get; set; }
        public string VatRate { get; set; }
        public double VatValue { get; set; }
    }
}