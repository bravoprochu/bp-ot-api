using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceExtraInfoChecked
    {
        public int InvoiceExtraInfoCheckedId { get; set; }
        public bool? Checked { get; set; }
        public DateTime? Date { get; set; }
        public string Info { get; set; }

        public InvoiceExtraInfo CmrChecked { get; set; }
        public int? CmrCheckedId { get; set; }

        public InvoiceExtraInfo RecivedChecked { get; set; }
        public int? RecivedCheckedId { get; set; }

        public InvoiceExtraInfo SentChecked { get; set; }
        public int? SentCheckedId { get; set; }

    }
}
