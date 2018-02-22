using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Jpk
{
    public class JpkSprzedaz
    {
        public JpkSprzedaz()
        {
            this.SprzedazWiersz = new List<JpkSprzedazWiersz>();
        }
        public List<JpkSprzedazWiersz> SprzedazWiersz { get; set; }
        public int LiczbaWierszySprzedazy => SprzedazWiersz.Count;
        public double PodatekNalezny {get;set;}
    }
}
