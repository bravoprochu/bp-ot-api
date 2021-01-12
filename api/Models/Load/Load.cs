using bp.kpir.DAO.Invoice;
using bp.shared.DTO;

namespace bp.ot.s.API.Models.Load
{
    public class Load:CreationInfo
    {
        public int LoadId { get; set; }

        public InvoiceBuy InvoiceBuy { get; set; }
        public InvoiceSell InvoiceSell { get; set; }
        public string LoadNo { get; set; }
        public string Info { get; set; }
        public LoadBuy LoadBuy { get; set; }
        public LoadSell LoadSell { get; set; }
        public LoadTransEu LoadTransEu { get; set; }

    }
}
