using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class PaymentTerms
    {
        public int PaymentTermsId { get; set; }
        public DateTime Day0 { get; set; }
        public String Description { get; set; }
        public InvoiceBuy InvoiceBuy { get; set; }
        public int? InvoiceBuyId { get; set; }
        public InvoiceSell InvoiceSell { get; set; }
        public int? InvoiceSellId { get; set; }
        public PaymentTerm PaymentTerm { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int? PaymentDays { get; set; }

    }
}