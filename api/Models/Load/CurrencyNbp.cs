using bp.ot.s.API.Entities.Dane.Invoice;
using System;

namespace bp.ot.s.API.Models.Load
{
    public class CurrencyNbp
    {
        public double Price { get; set; }
        public Currency Currency { get; set; }
        public double PlnValue { get; set; }
        public double Rate { get; set; }
        public DateTime RateDate{ get; set; }
    }
}