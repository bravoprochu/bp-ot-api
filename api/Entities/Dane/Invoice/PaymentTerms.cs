using bp.ot.s.API.Models.Load;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class PaymentTerms
    {
      
        public int PaymentTermsId { get; set; }
        public DateTime Day0 { get; set; }
        public int? PaymentDays { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentDescription { get; set; }
        public PaymentTerm PaymentTerm { get; set; }
        public int PaymentTermId { get; set; }



        public TradeInfo TradeInfo { get; set; }
        public int? TradeInfoId { get; set; }

        public InvoiceBuy InvoiceBuy { get; set; }
        public int? InvoiceBuyId { get; set; }

        public InvoiceSell InvoiceSell { get; set; }
        public int? InvoiceSellId { get; set; }

    }
}