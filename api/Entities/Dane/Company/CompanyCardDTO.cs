using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Company
{
    public class CompanyCardDTO
    {
        public CompanyCardDTO()
        {
            this.BankAccounts = new List<string>();
        }
        public int CompanyId { get; set; }
        public string ShortName { get; set; }
        public string VatId { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public List<string> BankAccounts { get; set; }
    }
}
