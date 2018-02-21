using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class CalcRatesDTO
    {
        public List<InvoiceLineDTO> Lines { get; set; }
        public bool IsCorrection { get; set; }
        //public List<InvoiceRatesGroupDTO> rates { get; set; }
    }
}
