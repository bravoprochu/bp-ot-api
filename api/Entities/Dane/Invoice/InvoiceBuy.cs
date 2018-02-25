using bp.ot.s.API.Models.Load;
using bp.shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceBuy: InvoiceCommon
    {
        public int InvoiceBuyId { get; set; }
        public Company.Company CompanySeller { get; set; }
        public int CompanySellerId { get; set; }
        public bool InvoiceReceived { get; set; }
        public DateTime? InvoiceReceivedDate { get; set; }

        public Load Load { get; set; }
        public int? LoadId { get; set; }

        public bool PaymentIsDone { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int PaymentTermsId { get; set; }
       
        
    }
}
