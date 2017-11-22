using System;

namespace bp.ot.s.API.Entities.Dane.TransportOffer
{
    public class TransportOfferAddress
    {
        public int TransportOfferAddressId { get; set; }
        public DateTime Date { get; set; }
        public string PostalCode { get; set; }
        public string Locality { get; set; }
        public TransportOffer Load { get; set; }
        public int? LoadId { get; set; }
        public TransportOffer Unload { get; set; }
        public int? UnloadId { get; set; }
    }
}