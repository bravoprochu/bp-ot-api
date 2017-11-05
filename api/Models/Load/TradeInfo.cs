using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Dane.Invoice;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class TradeInfo
    {
        public int TradeInfoId { get; set; }

        public DateTime Date { get; set; }
        public PaymentTerms PaymentTerms { get; set; }
        public int PaymentTermsId { get; set; }

        public CurrencyNbp CurrencyNbp { get; set; }
        public int CurrencyNbpId { get; set; }

        public Company Company { get; set; }
        public int CompanyId { get; set; }



        public LoadBuy LoadBuy { get; set; }
        public int? LoadBuyId { get; set; }
        public LoadSell LoadSell { get; set; }
        public int? LoadSellId { get; set; }

    }
}