using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Company
{
    public class BankAccountDTO
    {
        public int BankAccountId { get; set; }
        public string BankAccountInfo => $"{this.Type} {this.Swift} {this.Account_no}";
        public string Swift { get; set; }
        public string Account_no { get; set; }
        public string Type { get; set; }
    }
}
