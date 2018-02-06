using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Dane.Invoice;
using System;

namespace bp.ot.s.API.Models.Load
{
    public class TradeInfoDTO
    {
        public int TradeInfoId { get; set; }
        public CompanyDTO Company { get; set; }
        public DateTime Date { get; set; }
        public PaymentTermsDTO PaymentTerms { get; set; }
        public CurrencyNbpDTO Price { get; set; }

    }
}