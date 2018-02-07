using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Models.Load;
using bp.Pomocne.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceSellDTO
    {
        public InvoiceSellDTO()
        {
            this.InvoiceLines = new List<InvoiceLinesGroupDTO>();
        }
        public int InvoiceSellId { get; set; }
        public int BaseInvoiceId { get; set; }
        public CompanyDTO CompanyBuyer { get; set; }
        public CompanyDTO CompanySeller { get; set; }
        public CreationInfo CreationInfo { get; set; }
        public CurrencyDTO Currency { get; set; }
        public DateTime DateOfIssue { get; set; }
        public DateTime DateOfSell { get; set; }

        public InvoiceExtraInfoDTO ExtraInfo { get; set; }
        public string Info { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceOriginalNo { get; set; }
        public InvoiceTotalGroupDTO InvoiceTotal { get; set; }
        public bool IsCorrection { get; set; }
        public bool PaymentIsDone { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PaymentTermsDTO PaymentTerms { get; set; }
        public InvoiceRatesGroupDTO Rates { get; set; }
        public List<InvoiceLinesGroupDTO> InvoiceLines { get; set; }
        





    }
}
