namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceLineDTO
    {
        public int Invoice_pos_id { get; set; }
        public int? BaseInvoiceLineId { get; set; }
        public double Brutto_value { get; set; }
        public string CorrectionInfo { get; set; }
        public bool IsCorrected { get; set; }
        public string Measurement_unit { get; set; }
        public string Name { get; set; }
        public double Netto_value { get; set; }
        public string Pkwiu { get; set; }
        public int Quantity { get; set; }
        public double Unit_price { get; set; }
        public double Vat_unit_value { get; set; }
        public double Vat_value { get; set; }
        public string Vat_rate { get; set; }
    }
}