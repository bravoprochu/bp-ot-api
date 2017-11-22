using System;

namespace bp.ot.s.API.Entities.Dane.Transport
{
    public class TransportOfferAddressDTO
    {
        public int? TransportOfferAddressId { get; set; }
        public DateTime Date { get; set; }
        public string Locality { get; set; }
        public string PostalCode { get; set; }
       
    }
}