using bp.ot.s.API.Entities.Dane.Company;
using System.Collections.Generic;

namespace bp.ot.s.API.Models.Load
{
    public class LoadSellDTO
    {
        public LoadSellDTO()
        {
            this.Contact_persons_list = new List<CompanyEmployeeDTO>();
        }
        public BuyingInfoDTO Selling_info { get; set; }
        public CompanyDTO Principal { get; set; }
        public List<CompanyEmployeeDTO> Contact_persons_list { get; set; }
    }
}