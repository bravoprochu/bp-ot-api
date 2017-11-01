using bp.ot.s.API.Entities.Dane.Address;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class LoadRoutes
    {
        public LoadRoutes()
        {
            this.Pallets = new List<LoadPallets>();
        }

        public DateTime LoadingDate { get; set; }
        public string LoadingType { get; set; }
        public Address Address { get; set; }
        public GeoDTO Geo { get; set; }
        public string Info { get; set; }
        public List<LoadPallets> Pallets { get; set; }

    }
}