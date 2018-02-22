using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Jpk
{
    public class JpkZakup
    {
        public JpkZakup()
        {
            this.ZakupWiersz = new List<JpkZakup>();
        }

        public List<JpkZakup> ZakupWiersz { get; set; }
        public int LiczbaWierszyZakupow => ZakupWiersz.Count;
        public double PodatekNaliczony { get; set; }
    }
}
