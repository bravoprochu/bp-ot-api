using bp.ot.s.API.Models.Load;
using bp.Pomocne.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Transport
{
    public class TransportOfferDTO
    {
        public int? TransportOfferId { get; set; }
        public CreationInfo CreationInfo { get; set; }
        public string Info { get; set; }
        public bool InvoiceInPLN { get; set; }
        public int? InvoiceSellId { get; set; }
        public string InvoiceSellNo { get; set; }
        public TransportOfferAddressDTO Load { get; set; }
        public string OfferNo { get; set; }
        public TradeInfoDTO TradeInfo { get; set; }
        public TransportOfferAddressDTO Unload { get; set; }
    }
}
