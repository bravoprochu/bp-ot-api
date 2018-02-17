using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceRatesGroupDTO
    {
        public InvoiceRatesGroupDTO()
        {
            //this.Corrections = new InvoiceRatesValuesDTO();
            //this.Current = new InvoiceRatesValuesDTO();
            //this.Original = new InvoiceRatesValuesDTO();
        }

        public string VatRate { get; set; }
        public InvoiceRatesValuesDTO Corrections { get {
                var res = new InvoiceRatesValuesDTO();
                if (this.Current != null && this.Original != null)
                {
                    res.Brutto_value = this.Current.Brutto_value - this.Original.Brutto_value;
                    res.Vat_rate = this.Original.Vat_rate;
                    res.Netto_value = this.Current.Netto_value - this.Original.Netto_value;
                    res.Vat_value = this.Current.Vat_value - this.Original.Vat_value;
                }
                return res;

            } set
            {

            }
        }
        public InvoiceRatesValuesDTO Current { get; set; }
        public InvoiceRatesValuesDTO Original { get; set; }
    }
}