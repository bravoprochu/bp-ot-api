using bp.ot.s.API.Entities.Dane.Company;
using System;

namespace bp.ot.s.API.Models.Load
{
    public class BuyingInfoDTO
    {
        public CompanyDTO Company { get; set; }
        public DateTime Date { get; set; }
        public PaymentTermsDTO Payment_terms { get; set; }
        public CurrencyNbpDTO Price { get; set; }

    }
}