using bp.ot.s.API.Entities.Dane.Company;
using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class LaodSell
    {
        public BuyingInfo SellingInfo { get; set; }
        public Company Principal { get; set; }
        public List<CompanyEmployee> ContactPersonsList { get; set; }
    }
}