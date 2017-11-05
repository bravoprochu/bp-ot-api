using bp.ot.s.API.Models.Load;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace bp.ot.s.API.Entities.Dane.Company
{
    public class CompanyEmployee
    {
        public int CompanyEmployeeId { get; set; }

        public Company Company { get; set; }
        public int CompanyId { get; set; }

        public string Given_name { get; set; }
        public string Family_name { get; set; }
        public string Trans_id { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public bool Entitled { get; set; }
        public bool Hidden { get; set; }
        public bool Is_driver { get; set; }
        public bool Is_moderator { get; set; }


        public List<LoadSellContactPersons> LoadSellContactPersonsList { get; set; }

    }
}
