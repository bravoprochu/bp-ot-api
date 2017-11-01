using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class Currency
    {
        public int CurrencyId { get; set; }
        [MaxLength(3)]
        public string Name { get; set; }
        public string Description { get; set; }

    }
}