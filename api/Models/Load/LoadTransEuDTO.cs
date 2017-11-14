using bp.ot.s.API.Entities.Dane.Company;
using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class LoadTransEuDTO
    {
        public int? LoadTransEuId { get; set; }
        public CurrencyNbpDTO Price { get; set; }
        public CompanyDTO SellingCompany { get; set; }
        public string TransEuId { get; set; }
        public List<CompanyEmployeeDTO> ContactPersonsList { get; set; }
    }
}