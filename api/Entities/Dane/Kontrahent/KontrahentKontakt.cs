using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace bp.ot.s.API.Entities.Dane.Kontrahent
{
    public class KontrahentKontakt
    {
        [Key, ForeignKey("Kontrahent")]
        public int KontrahentId { get; set; }

        public string Email { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        public virtual Kontrahent Kontrahent { get; set; }

        [StringLength(15)]
        public string Tel { get; set; }

        [StringLength(15)]
        public string Tel2 { get; set; }

        [DataType(DataType.Url)]
        public string WWW { get; set; }



    }
}