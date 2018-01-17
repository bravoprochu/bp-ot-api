using bp.ot.s.API.Models.Load;
using bp.Pomocne.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceBuy: CreationInfo
    {
        public int InvoiceBuyId { get; set; }
        public Currency Currency { get; set; }
        public int CurrencyId { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string Info { get; set; }
        public string InvoiceNo { get; set; }
        public List<InvoicePos> InvoicePosList { get; set; }
        public bool InvoiceRecived { get; set; }
        public InvoiceTotal InvoiceTotal { get; set; }

        public Load Load { get; set; }
        public int? LoadId { get; set; }

        public bool PaymentIsDone { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PaymentTerms PaymentTerms { get; set; }
        public int PaymentTermsId { get; set; }

        public List<RateValue> RatesValuesList { get; set; }
        public Company.Company Seller { get; set; }
        public int SellerId { get; set; }
        public DateTime SellingDate { get; set; }
    }
}
