using bp.ot.s.API.Models.Load;
using System;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceSell: InvoiceCommon
    {
        public int InvoiceSellId { get; set; }
        public int? BaseInvoiceId { get; set; }
        public Company.Company Buyer { get; set; }
        public int BuyerId { get; set; }
        public int? CorrectiondId { get; set;}
        public string CorrectionTotalInfo { get; set; }
        public InvoiceExtraInfo ExtraInfo { get; set; }
        public bool IsCorrection { get; set; }
        public bool IsInactive { get; set; }
        public Load Load { get; set; }
        public int? LoadId { get; set; }
        public bool PaymentIsDone { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int PaymentTermsId { get; set; }
        public Company.Company Seller { get; set; }
        public int SellerId { get; set; }
        public TransportOffer.TransportOffer TransportOffer { get; set; }
        public int? TransportOfferId { get; set; }
    }
}
