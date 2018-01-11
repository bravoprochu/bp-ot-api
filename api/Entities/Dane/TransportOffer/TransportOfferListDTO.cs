using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.TransportOffer
{
    public class TransportOfferListDTO
    {
        public int TransportOfferId { get; set; }
        public string Currency { get; set; }
        public string DocumentNo { get; set; }

        public double Fracht { get; set; }
        public string LoadDate { get; set; }
        public string LoadPlace { get; set; }
        public string LoadPostalCode { get; set; }
        public string Seller { get; set; }
        public string StatusCode { get; set; }
        public string UnloadDate { get; set; }
        public string UnloadPlace { get; set; }
        public string UnloadPostalCode { get; set; }
    }
}
