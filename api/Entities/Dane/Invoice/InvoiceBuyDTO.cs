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
            this.Invoice_pos_list = new List<InvoicePosDTO>();
            this.Rates_values_list = new List<InvoiceRatesValuesDTO>();
        }

        public int Invoice_buy_id { get; set; }
        public CreationInfo CreationInfo { get; set; }
        public CurrencyDTO Currency { get; set; }
        public DateTime Date_of_issue { get; set; }
        public string Info { get; set; }
        public string Invoice_no { get; set; }
        public List<InvoicePosDTO> Invoice_pos_list { get; set; }
        public bool? InvoiceRecived { get; set; }
        public InvoiceTotalDTO Invoice_total { get; set; }
        public int? LoadId { get; set; }
        public string LoadNo { get; set; }
        public PaymentTermsDTO Payment_terms { get; set; }
        public List<InvoiceRatesValuesDTO> Rates_values_list { get; set; }
        public CompanyDTO Seller { get; set; }
        public DateTime Selling_date { get; set; }

    }
}
