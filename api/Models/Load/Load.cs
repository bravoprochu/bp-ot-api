using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Dane.Invoice;
using bp.shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Load
{
    public class Load:CreationInfo
    {
        public int LoadId { get; set; }

        public InvoiceBuy InvoiceBuy { get; set; }
        public InvoiceSell InvoiceSell { get; set; }
        public string LoadNo { get; set; }
        public string Info { get; set; }
        public LoadBuy LoadBuy { get; set; }
        public LoadSell LoadSell { get; set; }
        public LoadTransEu LoadTransEu { get; set; }

    }
}
