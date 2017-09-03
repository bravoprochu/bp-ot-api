using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace bp.ot.s.API.Entities.Dane.Kontrahent
{
    public class KontrahentAdres
    {
        public int KontrahentAdresId { get; set; }
        public bool IsDefault { get; set; }
        [StringLength(2)]
        public string KodKraju { get; set; }
        [StringLength(8)]
        public string KodPocztowy { get; set; }

        public int KontrahentRefId { get; set; }

        [ForeignKey("KontrahentRefId")]
        public virtual Kontrahent Kontrahent { get; set; }

        [StringLength(50)]
        public string Miejscowosc { get; set; }

        [StringLength(50)]
        public string Ulica { get; set; }

        [StringLength(30)]
        public string UlicaNr { get; set; }
        public string TypAdresu { get; set; }

    }
}