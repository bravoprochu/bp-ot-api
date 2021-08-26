using bp.kpir.DAO.Contractor;
using bp.kpir.DAO.Invoice;

namespace bp.ot.s.API.PDF.Models {
    
    public class InvoiceNotificationRequest {
        public LanguagesEnum Language { get; set; }
        public int InvoiceId { get; set; }
        public bool isInvoiceAttached { get; set; }

        public InvoicePaymentRemindDTO Payment { get; set; }
        public CompanyDTO Owner { get; set; }
        
    }
}