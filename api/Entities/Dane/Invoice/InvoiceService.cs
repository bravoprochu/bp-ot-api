﻿using bp.ot.s.API.Entities.Context;
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
        public readonly List<Currency> _currencyList;

        public InvoiceService(OfferTransDbContextDane db)
        {
            this._db = db;
            this._currencyList = _db.Currency.ToList();
        }




        public void CurrencyMapper(Currency dbCur, CurrencyDTO curDTO)
        {
            if (dbCur== null || (dbCur != null && curDTO.CurrencyId != dbCur.CurrencyId))
            {
                dbCur= _db.Currency.Find(curDTO.CurrencyId);
            }
        }

        public void CurrencyNbpMapper(CurrencyNbp dbCur, CurrencyNbpDTO curDTO)
        {
            if (dbCur.Currency == null || (dbCur.Currency != null && curDTO.Currency.CurrencyId != dbCur.Currency.CurrencyId)) {
                dbCur.Currency = _db.Currency.Find(curDTO.Currency.CurrencyId);
            }
            dbCur.PlnValue = curDTO.Pln_value;
            dbCur.Price = curDTO.Price;
            dbCur.Rate = curDTO.Rate;
            dbCur.RateDate = curDTO.Rate_date;
        }



        public IQueryable<InvoiceBuy> InvoiceBuyQueryable()
        {
            return _db.InvoiceBuy
                    .Include(i => i.Currency)
                    .Include(i => i.InvoicePosList)
                    .Include(i=>i.InvoiceTotal)
                    .Include(i => i.PaymentTerms).ThenInclude(i => i.PaymentTerm)
                    .Include(i => i.RatesValuesList)
                    .Include(i => i.Seller).ThenInclude(a => a.AddressList)
                    .Include(i => i.Seller).ThenInclude(e => e.EmployeeList)
                    .Include(i => i.Seller).ThenInclude(b => b.BankAccountList);
        }

        public IQueryable<InvoiceSell> InvoiceSellQueryable()
        {
            return this._db.InvoiceSell
                .Include(i => i.Buyer.AddressList)
                .Include(i => i.Buyer.EmployeeList)
                .Include(i => i.Buyer.BankAccountList)
                .Include(i => i.Currency)
                .Include(i=>i.ExtraInfo)
                .Include(i => i.InvoicePosList)
                .Include(i=>i.InvoiceTotal)
                .Include(i => i.PaymentTerms).ThenInclude(i => i.PaymentTerm)
                .Include(i => i.RatesValuesList)
                .Include(i => i.Seller.AddressList)
                .Include(i => i.Seller.EmployeeList)
                .Include(i => i.Seller.BankAccountList);
        }


        public void InvoiceTotalMapper(InvoiceTotal dbInv, InvoiceTotalDTO invDTO)
        {
            dbInv.TotalBrutto = invDTO.Total_brutto;
            dbInv.TotalNetto = invDTO.Total_netto;
            dbInv.TotalTax = invDTO.Total_tax;
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




   

        public CurrencyDTO EtoDTOCurrency(Currency curr)
        {
            var res = new CurrencyDTO();
            res.CurrencyId = curr.CurrencyId;
            res.Description = curr.Description;
            res.Name = curr.Name;
            return res;
        }

        public CurrencyNbpDTO EtDTOCurrencyNbp(CurrencyNbp cNbp)
        {
            var res = new CurrencyNbpDTO();
            res.Currency = this.EtoDTOCurrency(cNbp.Currency);
            res.Pln_value = cNbp.PlnValue;
            res.Price = cNbp.Price;
            res.Rate = cNbp.Rate;
            res.Rate_date = cNbp.RateDate;
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

        public PaymentTermsDTO EtDTOPaymentTerms(PaymentTerms pTerms)
        {
            var res = new PaymentTermsDTO();
            res.Day0 = pTerms.Day0;
            if (pTerms.PaymentTerm.IsPaymentDate)
            {
                res.PaymentDate = pTerms.PaymentDate.Value;
                res.PaymentDays = pTerms.PaymentDays.Value;
            }
            res.Description= pTerms.PaymentTerm.IsDescription ? pTerms.PaymentDescription : null;
            res.PaymentTerm = new PaymentTermDTO
            {
                IsDescription = pTerms.PaymentTerm.IsDescription,
                IsPaymentDate = pTerms.PaymentTerm.IsPaymentDate,
                Name = pTerms.PaymentTerm.Name,
                PaymentTermId = pTerms.PaymentTerm.PaymentTermId
            };
            res.PaymentTermsId = pTerms.PaymentTermsId;
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

        public InvoiceTotalDTO EtoDTOInvoiceTotal(InvoiceTotal inv)
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
        public void PaymentTermsMapper(PaymentTerms dbTerms, PaymentTermsDTO pDTO)
        {
            dbTerms.Day0 = pDTO.Day0;
            dbTerms.PaymentDescription = pDTO.PaymentTerm.IsDescription ? pDTO.Description : null;
            if (pDTO.PaymentTerm.IsPaymentDate)
            {
                dbTerms.PaymentDate = pDTO.PaymentDate.Value;
                dbTerms.PaymentDays = pDTO.PaymentDays.Value;
            }
            else
            {
                dbTerms.PaymentDate = null;
                dbTerms.PaymentDays = null;
            }
            if (dbTerms.PaymentTerm == null) //new paymentTerms
            {
                dbTerms.PaymentTerm = this._db.PaymentTerm.Find(pDTO.PaymentTerm.PaymentTermId);
            }
            else
            {
                if (dbTerms.PaymentTerm.PaymentTermId != pDTO.PaymentTerm.PaymentTermId)
                {
                    dbTerms.PaymentTerm = this._db.PaymentTerm.Find(pDTO.PaymentTerm.PaymentTermId);
                }
            }


            //return dbTerms;
        }
    }
}
