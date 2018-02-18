using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceSellStatsDTO
    {
        public CurrencyDTO Currency { get; set; }
        public InvoiceTotalDTO Total { get; set; }
        public double InvoiceValue { get; set; }
    }
}
