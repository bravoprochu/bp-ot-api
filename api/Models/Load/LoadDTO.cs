using bp.ot.s.API.Entities.Dane.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Load
{
    public class LoadDTO
    {
        public int? LoadId { get; set; }
        public string Info { get; set; }
        public string InvoiceSellNo { get; set; }
        public string LoadNo { get; set; }
        public LoadExtraInfoDTO LoadExtraInfo { get; set; }
        public LoadBuyDTO Buy { get; set; }
        public LoadSellDTO Sell { get; set; }
        public LoadTransEuDTO TransEu { get; set; }
    }
}
