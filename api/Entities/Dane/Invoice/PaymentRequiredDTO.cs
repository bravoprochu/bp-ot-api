using bp.ot.s.API.Entities.Dane.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class PaymentRequiredDTO
    {
        public DateTime PaymentDate { get; set; }
        public CompanyCardDTO Company { get; set; }
        public CurrencyDTO Currency { get; set; }
        public int InvoiceSellId { get; set; }
        public InvoiceTotalDTO InvoiceTotal { get; set; }
    }
}
