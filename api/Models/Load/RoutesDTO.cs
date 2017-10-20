﻿using bp.ot.s.API.Entities.Dane.Address;
using System;
using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class RoutesDTO
    {
        public RoutesDTO()
        {
            this.Pallets = new List<LoadPalletsDTO>();
        }

        public DateTime Loading_date { get; set; }
        public string Loading_type { get; set; }
        public AddressDTO Address { get; set; }
        public GeoDTO Geo { get; set; }
        public string Info { get; set; }
        public List<LoadPalletsDTO> Pallets { get; set; }
    }
}