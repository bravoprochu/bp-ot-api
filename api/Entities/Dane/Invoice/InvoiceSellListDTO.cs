using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceSellListDTO
    {
        public int Id { get; set; }

        public double Brutto { get; set; }
        public string DataSprzedazy { get; set; }
        public string DocumentNo { get; set; }
        public string Nabywca { get; set; }
        public double Netto { get; set; }
        public double Podatek { get; set; }
        public string Type { get; set; }
        public string Waluta { get; set; }
    }
}
