﻿using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Models.Load;
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
            this.Invoice_pos_list = new List<InvoicePosDTO>();
        }

        public int Invoice_sell_Id { get; set; }
        public CompanyDTO Buyer { get; set; }
        public CurrencyDTO Currency { get; set; }
        public DateTime Date_of_issue { get; set; }
        public InvoiceExtraInfoDTO Extra_info { get; set; }
        public string Info { get; set; }
        public string Invoice_no { get; set; }
        public InvoiceTotalDTO Invoice_total { get; set; }
        public PaymentTermsDTO Payment_terms { get; set; }
        public CompanyDTO Seller { get; set; }
        public DateTime Selling_date { get; set; }
        
        public List<InvoicePosDTO> Invoice_pos_list{ get; set; }
        public List<InvoiceRatesValuesDTO> Rates_values_list { get; set; }





    }
}
