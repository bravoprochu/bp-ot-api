using bp.ot.s.API.Entities.Dane.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Load
{
    public class LoadExtraInfoDTO:InvoiceExtraInfoDTO
    {
        public int? InvoiceBuyId { get; set; }
        public string InvoiceBuyNo { get; set; }
        public bool? InvoiceBuyRecived { get; set; }

    }
}
