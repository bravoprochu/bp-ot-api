namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceRatesValuesDTO
    {
        public int Invoice_rates_values_id { get; set; }
        public double Brutto_value { get; set; }
        public double Netto_value { get; set; }
        public string Vat_rate { get; set; }
        public double Vat_value { get; set; }

    }
}