using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceRatesGroupDTO
    {
        public InvoiceRatesGroupDTO()
        {
            this.Corrections = new InvoiceRatesValuesDTO();
            this.Current = new InvoiceRatesValuesDTO();
            this.Original = new InvoiceRatesValuesDTO();
        }

        public string VatRate { get; set; }
        public InvoiceRatesValuesDTO Corrections { get; set; }
        public InvoiceRatesValuesDTO Current { get; set; }
        public InvoiceRatesValuesDTO Original { get; set; }
    }
}