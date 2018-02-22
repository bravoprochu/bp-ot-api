using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Jpk
{
    public class JpkPodmiot
    {
        [MaxLength(10)]
        public string NIP { get; set; }
        public string PelnaNazwa { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
