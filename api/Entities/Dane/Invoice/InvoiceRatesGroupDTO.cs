using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceRatesGroupDTO
    {
        public InvoiceRatesGroupDTO()
        {
            this.Corrections = new List<InvoiceRatesValuesDTO>();
            this.Current = new List<InvoiceRatesValuesDTO>();
            this.Original = new List<InvoiceRatesValuesDTO>();
        }

        public List<InvoiceRatesValuesDTO> Corrections { get; set; }
        public List<InvoiceRatesValuesDTO> Current { get; set; }
        public List<InvoiceRatesValuesDTO> Original { get; set; }
    }
}