using bp.ot.s.API.Models.Load;
using System;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceExtraInfoDTO
    {

        public string CmrName { get; set; }
        public bool CmrRecived { get; set; }
        public DateTime? CmrRecivedDate { get; set; }

        public bool InvoiceSent { get; set; }
        public string InvoiceSentNo { get; set; }
        public DateTime? InvoiceRecivedDate { get; set; }

        public bool Is_load_no { get; set; }
        public bool Is_in_words { get; set; }
        public bool Is_tax_nbp_exchanged { get; set; }

        public string Load_no { get; set; }
        public int? LoadId { get; set; }
        public string Tax_exchanged_info { get; set; }
        public string Total_brutto_in_words { get; set; }



    }
}