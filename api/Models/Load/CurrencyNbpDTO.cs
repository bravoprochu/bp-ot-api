﻿using bp.ot.s.API.Entities.Dane.Invoice;
using System;

namespace bp.ot.s.API.Models.Load
{
    public class CurrencyNbpDTO
    {
        public double Price { get; set; }
        public CurrencyDTO Currency { get; set; }
        public double Pln_value { get; set; }
        public double Rate { get; set; }
        public DateTime Rate_date { get; set; }
        

    }
}