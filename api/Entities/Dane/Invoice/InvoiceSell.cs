using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Models.Load;
using bp.Pomocne.DTO;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceSell: CreationInfo
    {

        public int InvoiceSellId { get; set; }
        public Company.Company Buyer { get; set; }
        public int BuyerId { get; set; }

        public Currency Currency { get; set; }
        public int CurrencyId { get; set; }
        public DateTime DateOfIssue { get; set; }

        public InvoiceExtraInfo ExtraInfo { get; set; }

        public string Info { get; set; }
        public string InvoiceNo { get; set; }
        
        public InvoiceTotal InvoiceTotal { get; set; }

        public Load Load { get; set; }
        public int? LoadId { get; set; }

        public PaymentTerms PaymentTerms { get; set; }
        public int PaymentTermsId { get; set; }


        public List<RateValue> RatesValuesList { get; set; }
        public Company.Company Seller { get; set; }
        public int SellerId { get; set; }
        public DateTime SellingDate { get; set; }

        public TransportOffer.TransportOffer TransportOffer { get; set; }
        public int? TransportOfferId { get; set; }

        public List<InvoicePos> InvoicePosList { get; set; }
    }
}
