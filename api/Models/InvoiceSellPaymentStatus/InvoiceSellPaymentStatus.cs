using System;
using System.Collections.Generic;
using bp.kpir.DAO.Invoice;

namespace bp.ot.s.API.Models.InvoiceSellPaymentStatus
{
    public class InvoiceSellPaymentStatus
    {
        public InvoiceSellPaymentStatus()
        {
            this.Unpaid = new List<InvoicePaymentRemindDTO>();
            this.UnpaidStats = new List<InvoiceSellStatsDTO>();
            this.UnpaidOverdue = new List<InvoicePaymentRemindDTO>();
            this.UnpaidOverdueStats = new List<InvoiceSellStatsDTO>();
            this.NotConfirmed = new List<InvoicePaymentRemindDTO>();
            this.NotConfirmedStats = new List<InvoiceSellStatsDTO>();

        }

        public List<InvoicePaymentRemindDTO> Unpaid { get; set; }

        public List<InvoiceSellStatsDTO> UnpaidStats { get; set; }


        public List<InvoicePaymentRemindDTO> UnpaidOverdue { get; set; }

        public List<InvoiceSellStatsDTO> UnpaidOverdueStats { get; set; }


        public List<InvoicePaymentRemindDTO> NotConfirmed { get; set; }

        public List<InvoiceSellStatsDTO> NotConfirmedStats { get; set; }

    }
}
