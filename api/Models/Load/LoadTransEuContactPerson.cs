﻿using bp.kpir.DAO.Contractor;

namespace bp.ot.s.API.Models.Load
{
    public class LoadTransEuContactPerson
    {
        public CompanyEmployee CompanyEmployee { get; set; }
        public int CompanyEmployeeId { get; set; }
        public LoadTransEu LoadTransEu { get; set; }
        public int LoadTransEuId { get; set; }
    }
}