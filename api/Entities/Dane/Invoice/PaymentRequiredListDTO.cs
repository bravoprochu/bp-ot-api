using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class PaymentRequiredListDTO
    {

        public PaymentRequiredListDTO()
        {
            this.InvoiceSell = new List<InvoiceSellDTO>();
        }

        public DateTime Date { get; set; }
        public List<InvoiceSellDTO> InvoiceSell { get; set; }
    }
}
