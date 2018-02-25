using bp.ot.s.API.Entities.Dane.Company;
using bp.shared.DTO;
using bp.ot.s.API.Models.Load;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceBuyDTO:InvoiceCommonDTO
    {
        public int InvoiceBuyId { get; set; }
        public CompanyDTO CompanySeller { get; set; }
        public bool? IsInvoiceReceived { get; set; }
        public DateTime? InvoiceReceivedDate { get; set; }
        public int? LoadId { get; set; }
        public string LoadNo { get; set; }
        public bool PaymentIsDone { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
