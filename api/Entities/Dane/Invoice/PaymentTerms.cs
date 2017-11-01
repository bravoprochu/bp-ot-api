using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class PaymentTerms
    {
        public int? PaymentDays { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentDescription { get; set; }
        public PaymentTerm PaymentTerm { get; set; }

    }
}