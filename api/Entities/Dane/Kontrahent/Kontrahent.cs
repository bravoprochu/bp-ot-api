using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace bp.ot.s.API.Entities.Dane.Kontrahent
{
    public class Kontrahent
    {
        public Kontrahent()
        {
            this.KontrahentAdres = new HashSet<KontrahentAdres>();
            this.KontrahentBank = new HashSet<KontrahentBank>();
        }

        public int KontrahentId { get; set; }

        [StringLength(30)]
        public string Skrot { get; set; }

        [StringLength(100)]
        public string Nazwa { get; set; }

        [StringLength(30)]
        public string Imie { get; set; }

        [StringLength(50)]
        public string Nazwisko { get; set; }

        [StringLength(12)]
        public string NIP { get; set; }
        public string Uwagi { get; set; }

        public virtual KontrahentKontakt KontrahentKontakt { get; set; }

        public virtual ICollection<KontrahentAdres> KontrahentAdres { get; set; }
        public virtual ICollection<KontrahentBank> KontrahentBank { get; set; }

    }
}