using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bp.ot.s.API.Entities.Dane.Company;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceSell
    {
        public int InvoiceSellId { get; set; }
        public Company.Company Buyer { get; set; }
        public Currency Currency { get; set; }
        public DateTime DateOfIssue { get; set; }
        public bool ExtraInfo_IsLoadNo { get; set; }
        public bool ExtraInfo_IsInWords { get; set; }
        public bool ExtraInfo_IsTaxNbpExchanged { get; set; }
        public string ExtraInfo_LoadNo { get; set; }
        public string ExtraInfo_TaxExchangedInfo { get; set; }
        public string ExtraInfo_TotalBruttoInWords { get; set; }
        public string Info { get; set; }
        public string InvoiceNo { get; set; }
        public List<InvoicePos> InvoicePosList { get; set; }
        public double TotalBrutto { get; set; }
        public double TotalNetto { get; set; }
        public double TotalTax { get; set; }
        public PaymentTerms PaymentTerms { get; set; }
        public List<RateValue> RatesValuesList { get; set; }
        public Company.Company Seller { get; set; }
        public DateTime SellingDate { get; set; }
    }
}
