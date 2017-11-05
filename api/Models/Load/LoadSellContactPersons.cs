using bp.ot.s.API.Entities.Dane.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.Load
{
    public class LoadSellContactPersons
    {
        public CompanyEmployee CompanyEmployee { get; set; }
        public int CompanyEmployeeId { get; set; }
        public LoadSell LoadSell{ get; set; }
        public int LoadSellId { get; set; }
    }
}
