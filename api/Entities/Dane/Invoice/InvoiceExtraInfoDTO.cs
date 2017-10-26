using bp.ot.s.API.Models.Load;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceExtraInfoDTO
    {
        public bool Is_load_no { get; set; }
        public bool Is_in_words { get; set; }
        public bool Is_tax_nbp_exchanged { get; set; }
        public string Load_no { get; set; }
        public string Tax_exchanged_info { get; set; }
        public CurrencyNbpDTO Tax_nbp { get; set; }
        public string Total_brutto_in_words { get; set; }



    }
}