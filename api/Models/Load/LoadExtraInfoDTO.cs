using bp.kpir.DAO.Invoice;

namespace bp.ot.s.API.Models.Load
{
    public class LoadExtraInfoDTO:InvoiceExtraInfoDTO
    {
        public int? InvoiceBuyId { get; set; }
        public string InvoiceBuyNo { get; set; }
        public bool? InvoiceBuyRecived { get; set; }

    }
}
