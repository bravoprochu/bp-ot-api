﻿namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class PaymentTerm
    {
        public int PaymentTermId { get; set; }
        public string Name { get; set; }
        public bool IsDescription { get; set; }
        public bool IsPaymentDate { get; set; }

    }
}