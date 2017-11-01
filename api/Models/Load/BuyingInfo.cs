using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Dane.Invoice;
using System;

namespace bp.ot.s.API.Models.Load
{
    public class BuyingInfo
    {
        public Company Company { get; set; }
        public DateTime Date { get; set; }
        public PaymentTerms PaymentTerms { get; set; }
        public CurrencyNbp CurrencyNbp { get; set; }
    }
}