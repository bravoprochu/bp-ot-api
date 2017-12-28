using bp.ot.s.API.Entities.Dane.Invoice;
using bp.Pomocne.DTO;
using bp.ot.s.API.Models.Load;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.TransportOffer
{
    public class TransportOffer : CreationInfo
    {
        public int TransportOfferId { get; set; }
        public Company.Company Company { get; set; }
        public int CompanyId { get; set; }
        public CurrencyNbp CurrencyNbp { get; set; }
        public int CurrencyNbpId { get; set; }
        public DateTime Date { get; set; }
        public string Info { get; set; }
        public InvoiceSell InvoiceSell { get; set; }
        public int? InvoiceSellId { get; set; }
        public TransportOfferAddress Load { get; set; }
        public string OfferNo { get; set; }
        public PaymentTerms PaymentTerms { get; set; }
        public int PaymentTermsId { get; set; }
        public TransportOfferAddress Unload { get; set; }


    }
}
