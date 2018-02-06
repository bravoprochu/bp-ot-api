using bp.ot.s.API.Entities.Dane.Company;
using bp.Pomocne.DTO;
using bp.ot.s.API.Models.Load;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceBuyDTO
    {
        public InvoiceBuyDTO()
        {
            this.InvoiceLines = new List<InvoiceLineDTO>();
            this.Rates_values_list = new List<InvoiceRatesValuesDTO>();
        }

        public int Invoice_buy_id { get; set; }
        public CreationInfo CreationInfo { get; set; }
        public CurrencyDTO Currency { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string Info { get; set; }
        public string InvoiceNo { get; set; }
        public List<InvoiceLineDTO> InvoiceLines { get; set; }
        public bool? InvoiceRecived { get; set; }
        public DateTime? InvoiceReciveDate { get; set; }
        public InvoiceTotalDTO InvoiceTotal { get; set; }
        public int? LoadId { get; set; }
        public string LoadNo { get; set; }
        public bool PaymentIsDone { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PaymentTermsDTO PaymentTerms { get; set; }
        public List<InvoiceRatesValuesDTO> Rates_values_list { get; set; }
        public CompanyDTO Seller { get; set; }
        public DateTime dateOfSell { get; set; }

    }
}
