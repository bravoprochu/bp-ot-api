using System.Collections.Generic;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceLinesGroupDTO
    {
        public InvoiceLineDTO Corrections { get; set; }
        public InvoiceLineDTO Current { get; set; }
        public InvoiceLineDTO Original { get; set; }
    }
}