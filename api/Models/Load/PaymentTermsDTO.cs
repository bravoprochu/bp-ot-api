using System;

namespace bp.ot.s.API.Models.Load
{
    public class PaymentTermsDTO
    {
        public string PaymentTermsCombined { get {

                string description = this.PaymentTerm.IsDescription ? ", "+ this.Description : null;
                string paymentDate = this.PaymentTerm.IsPaymentDate ? $", {PaymentDays} dni,  termin płatności: " + this.PaymentDate.ToShortDateString() : null;
                return $"{this.PaymentTerm.Name}{paymentDate}{description}";
            } }
        public string Description { get; set; }
        public DateTime Day0 { get; set; }
        public int? PaymentId { get; set; }
        public PaymentTermDTO PaymentTerm { get; set; }

        public DateTime PaymentDate { get; set; }
        public int PaymentDays { get; set; }

    }
}