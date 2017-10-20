using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Load
{
    public class LoadDTO
    {
        public string LoadNo { get; set; }
        public LoadBuyDTO Buy { get; set; }
        public LoadSellDTO Sell { get; set; }
    }
}
