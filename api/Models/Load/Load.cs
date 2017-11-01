using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Load
{
    public class Load
    {
        public int LoadId { get; set; }
        public LoadBuy LoadBuy { get; set; }
        public LaodSell LoadSell { get; set; }
    }
}
