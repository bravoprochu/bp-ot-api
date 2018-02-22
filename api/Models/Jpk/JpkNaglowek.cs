using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Jpk
{
    public class JpkNaglowek
    {
        public string KodFormularza { get; set; }
        public int WariantFormularza { get; set; }
        public string CelZlozenia { get; set; }
        public DateTime DataWytworzeniaJPK { get; set; }
        public DateTime DataOd { get; set; }
        public DateTime DataDo { get; set; }
        public string NazwaSystemu { get; set; }
        //public string KodSystemowy { get; set; }
        //public string WersjaSchemy { get; set; }
      

    }
}
