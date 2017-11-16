using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceBuy
    {
        public int InvoiceBuyId { get; set; }
        public Currency Currency { get; set; }
        public int CurrencyId { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string Info { get; set; }
        public string InvoiceNo { get; set; }
        public List<InvoicePos> InvoicePosList { get; set; }

        public InvoiceTotal InvoiceTotal { get; set; }

        public PaymentTerms PaymentTerms { get; set; }
        public int PaymentTermsId { get; set; }

        public List<RateValue> RatesValuesList { get; set; }
        public Company.Company Seller { get; set; }
        public int SellerId { get; set; }
        public DateTime SellingDate { get; set; }
    }
}
