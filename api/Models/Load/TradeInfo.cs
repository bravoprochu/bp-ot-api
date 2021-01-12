using bp.kpir.DAO.Contractor;
using bp.kpir.DAO.CurrenciesNbp;
using bp.kpir.DAO.Invoice;
using System;

namespace bp.ot.s.API.Models.Load
{
    public class TradeInfo
    {
        public int TradeInfoId { get; set; }

        public DateTime Date { get; set; }
        public PaymentTerms PaymentTerms { get; set; }
        public CurrencyNbp CurrencyNbp { get; set; }
        public Company Company { get; set; }
        public int CompanyId { get; set; }

        public LoadBuy LoadBuy { get; set; }
        public int? LoadBuyId { get; set; }
        public LoadSell LoadSell { get; set; }
        public int? LoadSellId { get; set; }

    }
}