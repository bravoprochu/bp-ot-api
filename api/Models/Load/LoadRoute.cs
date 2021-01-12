using bp.kpir.DAO.Addresses;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class LoadRoute
    {
        public LoadRoute()
        {
            this.Pallets = new List<LoadRoutePallet>();
        }

        public int LoadRouteId { get; set; }
        public DateTime LoadingDate { get; set; }
        public Address Address { get; set; }
        //public LoadRouteGeo Geo { get; set; }
        public string Info { get; set; }
        //isLoadingType == "Załadunek" : "Rozładunek"
        public bool IsLoadingType { get; set; }
        public List<LoadRoutePallet> Pallets { get; set; }

        public LoadBuy LoadBuy { get; set; }
        public int LoadBuyId { get; set; }

    }
}