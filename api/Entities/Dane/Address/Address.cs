using bp.ot.s.API.Models.Load;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Address
{
    public class Address
    {
        public int AddressId { get; set; }
        public string Address_type { get; set; }


        [MaxLength(2)]
        public string Country { get; set; }
        public string Locality { get; set; }
        [MaxLength(12)]
        public string Postal_code { get; set; }
        public string Street_address { get; set; }
        public string Street_number { get; set; }


        public Company.Company Company { get; set; }
        public int? CompanyId { get; set; }


        public LoadRoute LoadRoute { get; set; }
        public int? LoadRouteId { get; set; }

    }
}
