using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceExtraInfoCheckedDTO
    {
        public int? InvoiceExtraInfoCheckedId { get; set; }
        public bool? Checked { get; set; }
        public DateTime? Date { get; set; }
        public string Info { get; set; }
    }
}
