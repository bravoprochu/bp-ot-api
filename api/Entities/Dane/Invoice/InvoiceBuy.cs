using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceBuy
    {
        public InvoiceBuy()
        {
            this.InvoicePosList = new List<InvoicePos>();
            this.RatesValuesList = new List<RateValue>();
        }

        public int InvoiceBuyId { get; set; }
        public Currency Currency { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string Info { get; set; }
        public string InvoiceNo { get; set; }
        public List<InvoicePos> InvoicePosList { get; set; }
        public double TotalBrutto { get; set; }
        public double TotalNetto { get; set; }
        public double TotalTax { get; set; }

        public int? PaymentDays { get; set; }
        public DateTime? PaymentDate { get; set;}
        public string PaymentDescription { get; set; }
        public PaymentTerm PaymentTerm { get; set; }

        //public int PaymentTermsId { get; set; }
        public List<RateValue> RatesValuesList { get; set; }
        public Company.Company Seller { get; set; }
        public DateTime SellingDate { get; set; }
    }
}
