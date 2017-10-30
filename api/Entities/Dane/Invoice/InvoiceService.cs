using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Models.Load;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceService
    {
        private readonly OfferTransDbContextDane _db;


        public InvoiceService(OfferTransDbContextDane db)
        {
            this._db = db;
        }



        public InvoicePos NewInvoicePosBasedOnDTOMapper(InvoicePosDTO posDTO)
        {
            var pos = new InvoicePos();

            pos.BruttoValue = posDTO.Brutto_value;
            pos.MeasurementUnit = posDTO.Measurement_unit;
            pos.Name = posDTO.Name;
            pos.NettoValue = posDTO.Netto_value;
            pos.Pkwiu = posDTO.Pkwiu;
            pos.Quantity = posDTO.Quantity;
            pos.UnitPrice = posDTO.Unit_price;
            pos.VatRate = posDTO.Vat_rate;
            pos.VatUnitValue = posDTO.Vat_unit_value;
            pos.VatValue = posDTO.Vat_value;
            return pos;
        }

        public RateValue NewInvoiceRateValueBasedOnDTOMapper(InvoiceRatesValuesDTO rateDTO)
        {
            var rate = new RateValue();

            rate.BruttoValue = rateDTO.Brutto_value;
            rate.NettoValue = rateDTO.Netto_value;
            rate.VatRate = rateDTO.Vat_rate;
            rate.VatValue = rateDTO.Vat_value;

            return rate;
        }

        public void PaymentTermsBuyBasedOnDTOMapper(PaymentTermsDTO pDTO, InvoiceBuy inv)
        {


            //dbTerms.Day0 = pDTO.Day0;
            //dbTerms.Description = pDTO.PaymentTerm.IsDescription ? pDTO.Description : null;
            //if (pDTO.PaymentTerm.IsPaymentDate) {
            //    dbTerms.PaymentDate = pDTO.PaymentDate.Value;
            //    dbTerms.PaymentDays = pDTO.PaymentDays.Value;
            //}
            //else {
            //    pDTO.PaymentDate = null;
            //    pDTO.PaymentDays = null;
            //}
            
            //if (dbTerms.PaymentTerm == null) {
            //    dbTerms.PaymentTerm = this._db.PaymentTerm.Where(w => w.PaymentTermId == pDTO.PaymentTerm.PaymentTermId).FirstOrDefault();
            //}
            //if (dbTerms.PaymentTerm!=null && dbTerms.PaymentTerm.PaymentTermId != pDTO.PaymentTerm.PaymentTermId)
            //{
            //    dbTerms.PaymentTerm = this._db.PaymentTerm.Where(w => w.PaymentTermId == pDTO.PaymentTerm.PaymentTermId).FirstOrDefault();
            //}

            //return dbTerms;
        }

        public CurrencyDTO EtoDTOCurrency(Currency curr)
        {
            var res = new CurrencyDTO();
            res.CurrencyId = curr.CurrencyId;
            res.Description = curr.Description;
            res.Name = curr.Name;
            return res;
        }

        public InvoicePosDTO EtoDTOInvoicePos(InvoicePos pos)
        {
            var res = new InvoicePosDTO();
            res.Brutto_value = pos.BruttoValue;
            res.Invoice_pos_id = pos.InvoicePosId;
            res.Measurement_unit = pos.MeasurementUnit;
            res.Name = pos.Name;
            res.Netto_value = pos.NettoValue;
            res.Pkwiu = pos.Pkwiu;
            res.Quantity = pos.Quantity;
            res.Unit_price = pos.UnitPrice;
            res.Vat_rate = pos.VatRate;
            res.Vat_unit_value = pos.VatUnitValue;
            res.Vat_value = pos.VatValue;

            return res;
        }
        public PaymentTermsDTO EtoDTOPaymentTermsInvoiceBuy(InvoiceBuy inv)
        {
            var res = new PaymentTermsDTO();
            res.Day0 = inv.SellingDate;
            res.Description = inv.PaymentDescription;
            if (inv.PaymentDays.HasValue)
            {
                res.PaymentDate = inv.PaymentDate.Value;
            }
            if (inv.PaymentDays.HasValue)
            {
                res.PaymentDays = inv.PaymentDays.Value;
            }
            res.PaymentTerm = new PaymentTermDTO
            {
                IsDescription = inv.PaymentTerm.IsDescription,
                IsPaymentDate = inv.PaymentTerm.IsPaymentDate,
                Name = inv.PaymentTerm.Name,
                PaymentTermId = inv.PaymentTerm.PaymentTermId
            };
            return res;
        }

        public PaymentTermsDTO EtoDTOPaymentTermsInvoiceSell(InvoiceSell inv)
        {
            var res = new PaymentTermsDTO();
            res.Day0 = inv.SellingDate;
            res.Description = inv.PaymentDescription;
            if (inv.PaymentDays.HasValue)
            {
                res.PaymentDate = inv.PaymentDate.Value;
            }
            if (inv.PaymentDays.HasValue)
            {
                res.PaymentDays = inv.PaymentDays.Value;
            }
            res.PaymentTerm = new PaymentTermDTO
            {
                IsDescription = inv.PaymentTerm.IsDescription,
                IsPaymentDate = inv.PaymentTerm.IsPaymentDate,
                Name = inv.PaymentTerm.Name,
                PaymentTermId = inv.PaymentTerm.PaymentTermId
            };
            return res;
        }


        public InvoiceRatesValuesDTO EtoDTORateValue(RateValue rate)
        {
            var res = new InvoiceRatesValuesDTO();

            res.Brutto_value = rate.BruttoValue;
            res.Invoice_rates_values_id = rate.RateValueId;
            res.Netto_value = rate.NettoValue;
            res.Vat_rate = rate.VatRate;
            res.Vat_value = rate.VatValue;

            return res;
        }

        public InvoiceTotalDTO EtoDTOInvoiceTotal(InvoiceSell inv)
        {
            var res = new InvoiceTotalDTO();
            res.Total_brutto = inv.TotalBrutto;
            res.Total_netto = inv.TotalNetto;
            res.Total_tax = inv.TotalTax;
            return res;
        }

        public void InvoicePosMapperFromDTO(InvoicePos dbPos, InvoicePosDTO posDTO)
        {
            dbPos.BruttoValue = posDTO.Brutto_value;
            dbPos.MeasurementUnit = posDTO.Measurement_unit;
            dbPos.Name = posDTO.Name;
            dbPos.NettoValue = posDTO.Netto_value;
            dbPos.Pkwiu = posDTO.Pkwiu;
            dbPos.Quantity = posDTO.Quantity;
            dbPos.UnitPrice = posDTO.Unit_price;
            dbPos.VatRate = posDTO.Vat_rate;
            dbPos.VatUnitValue = posDTO.Vat_unit_value;
            dbPos.VatValue = posDTO.Vat_value;
        }

        public void InvoiceTaxValueMapperFromDTO(RateValue dbRate, InvoiceRatesValuesDTO rateDTO)
        {
            dbRate.BruttoValue = rateDTO.Brutto_value;
            dbRate.NettoValue = rateDTO.Netto_value;
            dbRate.VatRate = rateDTO.Vat_rate;
            dbRate.VatValue = rateDTO.Vat_value;
        }

    }
}
