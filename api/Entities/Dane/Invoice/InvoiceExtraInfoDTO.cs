using bp.ot.s.API.Models.Load;
using System;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceExtraInfoDTO
    {
        public InvoiceExtraInfoCheckedDTO Cmr { get; set; }
        public InvoiceExtraInfoCheckedDTO Recived { get; set; }
        public InvoiceExtraInfoCheckedDTO Sent { get; set; }
        public CurrencyNbpDTO CurrencyNbp { get; set; }
        public int? InvoiceSellId { get; set; }
        public string InvoiceSellNo { get; set; }
        public bool Is_load_no { get; set; }
        public bool Is_in_words { get; set; }
        public bool IsSigningPlace { get; set; }
        public bool Is_tax_nbp_exchanged { get; set; }
        public string LoadNo { get; set; }
        public int? LoadId { get; set; }
        public string Tax_exchanged_info { get; set; }
        public string Total_brutto_in_words { get; set; }
        public int? TransportOfferId { get; set; }
        public string TransportOfferNo { get; set;}
    }
}