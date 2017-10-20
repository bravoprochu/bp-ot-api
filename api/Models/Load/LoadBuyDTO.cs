﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Load
{
    public class LoadBuyDTO
    {
        public LoadBuyDTO()
        {
            this.Routes = new List<RoutesDTO>();
        }
        public BuyingInfoDTO Buying_info { get; set; }
        public LoadInfoDTO Load_info { get; set; }
        public List<RoutesDTO> Routes { get; set; }

    }
}