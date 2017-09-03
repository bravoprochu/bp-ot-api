using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace bp.ot.s.API.Entities.Dane.Kontrahent
{
    public class KontrahentBank
    {
        public int KontrahentBankId { get; set; }
        public bool IsDefault { get; set; }

        public int KontrahentRefId { get; set; }
        [ForeignKey("KontrahentRefId")]
        public virtual Kontrahent Kontrahent { get; set; }

        [StringLength(28)]
        public string NumerRachunku { get; set; }
        [StringLength(11)]
        public string Swift { get; set; }
        public string Uwagi { get; set; }
    }
}