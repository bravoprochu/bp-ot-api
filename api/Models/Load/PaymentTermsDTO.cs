using System;

namespace bp.ot.s.API.Models.Load
{
    public class PaymentTermsDTO
    {
        public string Description { get; set; }
        public DateTime Day0 { get; set; }
        public int? PaymentId { get; set; }
        public PaymentTermDTO PaymentTerm { get; set; }

        public DateTime PaymentDate { get; set; }
        public int PaymentDays { get; set; }

    }
}