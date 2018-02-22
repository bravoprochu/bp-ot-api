using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Jpk
{
    public class JpkZakupWiersz
    {
        public int LpZakupu { get; set; }
        [MaxLength(10)]
        public string NrDostawcy { get; set; }
        public string NazwaDostawcy { get; set; }
        public string AdresDostawcy { get; set; }
        public string DowodZakupu { get; set; }
        public DateTime DataZakupu { get; set; }
        public DateTime DataWplywu { get; set; }
        public double? K_43 { get; set; }
        public double? K_44 { get; set; }
        public double? K_45 { get; set; }
        public double? K_46 { get; set; }
        public double? K_47 { get; set; }
        public double? K_48 { get; set; }
        public double? K_49 { get; set; }
        public double? K_50 { get; set; }
    }
}
