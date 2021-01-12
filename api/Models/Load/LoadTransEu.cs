using bp.kpir.DAO.Contractor;
using bp.kpir.DAO.CurrenciesNbp;
using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class LoadTransEu
    {
        public int LoadTransEuId { get; set; }
        public CurrencyNbp Price { get; set; }
        public Company SellingCompany { get; set; }
        public int SellingCompanyId { get; set; }

        public string TransEuId { get; set; }


        public Load Load { get; set; }
        public int LoadId { get; set; }

        public List<LoadTransEuContactPerson> ContactPersonsList { get; set; }
    }
}