﻿using System.ComponentModel.DataAnnotations;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoicePos
    {
        public int InvoicePosId { get; set; }
        public double BruttoValue { get; set; }
        public InvoiceSell InvoiceSell { get; set; }
        public InvoiceBuy InvoiceBuy { get; set; }
        public string Name { get; set; }
        [MaxLength(10)]
        public string MeasurementUnit { get; set; }
        public double NettoValue { get; set; }
        public string Pkwiu { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double VatUnitValue { get; set; }
        public double VatValue { get; set; }
        [MaxLength(15)]
        public string VatRate { get; set; }
    }
}