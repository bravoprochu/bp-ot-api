using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Models.Load;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bp.ot.s.API.Entities.Dane.Company;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public class InvoiceService
    {
        private readonly OfferTransDbContextDane _db;
        public readonly List<Currency> _currencyList;

        public InvoiceService(OfferTransDbContextDane db, Company.CompanyService companyService)
        {
            this._db = db;
            this._currencyList = _db.Currency.ToList();
        }


        public void CurrencyNbpMapper(CurrencyNbp dbCur, CurrencyNbpDTO curDTO)
        {
            if (dbCur.Currency == null || (dbCur.Currency != null && curDTO.Currency.CurrencyId != dbCur.Currency.CurrencyId))
            {
                dbCur.Currency = _db.Currency.Find(curDTO.Currency.CurrencyId);
            }
            dbCur.PlnValue = curDTO.Pln_value;
            dbCur.Price = curDTO.Price;
            dbCur.Rate = curDTO.Rate;
            dbCur.RateDate = curDTO.Rate_date;
        }

        public LoadExtraInfoDTO EtoDTOExtraInfo(InvoiceExtraInfo inv)
        {
            var res = new LoadExtraInfoDTO();
            res.Is_in_words = false;
            if (!string.IsNullOrWhiteSpace(inv.LoadNo))
            {
                res.Is_load_no = true;
                res.LoadNo = inv.LoadNo;
            }
            else
            {
                res.Is_load_no = false;
            }

            if (!string.IsNullOrWhiteSpace(inv.TaxExchangedInfo))
            {
                res.Is_tax_nbp_exchanged = true;
                res.Tax_exchanged_info = inv.TaxExchangedInfo;
            }
            else
            {
                res.Is_tax_nbp_exchanged = false;
            }

            if (inv.Cmr != null)
            {

            }

            res.Cmr = inv.Cmr != null ? this.EtoDTOExtraInfoChecked(inv.Cmr) : new InvoiceExtraInfoCheckedDTO();
            res.Recived = inv.Recived != null ? this.EtoDTOExtraInfoChecked(inv.Recived) : new InvoiceExtraInfoCheckedDTO();
            res.Sent = inv.Sent != null ? this.EtoDTOExtraInfoChecked(inv.Sent) : new InvoiceExtraInfoCheckedDTO();

            //if (inv.Cmr != null) {
            //    res.Cmr = this.EtoDTOExtraInfoChecked(inv.Cmr);
            //}
            //if (inv.Recived != null) {
            //    res.Recived = this.EtoDTOExtraInfoChecked(inv.Recived);
            //}
            //if (inv.Sent != null) {
            //    res.Sent = this.EtoDTOExtraInfoChecked(inv.Sent);
            //}

            res.InvoiceSellId = inv.InvoiceSellId;
            res.InvoiceSellNo = inv.InvoiceSell.InvoiceNo;

            if (inv.InvoiceSell.TransportOffer != null)
            {
                res.TransportOfferId = inv.InvoiceSell.TransportOffer.TransportOfferId;
                res.TransportOfferNo = inv.InvoiceSell.TransportOffer.OfferNo;
            }

            return res;
        }
        public InvoiceExtraInfoCheckedDTO EtoDTOExtraInfoChecked(InvoiceExtraInfoChecked db)
        {
            var res = new InvoiceExtraInfoCheckedDTO();
            res.InvoiceExtraInfoCheckedId = db.InvoiceExtraInfoCheckedId;
            res.Checked = db.Checked;
            res.Date = db.Date;
            res.Info = db.Info;

            return res;
        }

        public void InvoiceExtraInfoMapper(InvoiceExtraInfo dbInv, InvoiceExtraInfoDTO infoDTO)
        {
            dbInv.LoadNo = infoDTO.Is_load_no ? infoDTO.LoadNo : null;
            dbInv.TaxExchangedInfo = infoDTO.Is_tax_nbp_exchanged ? infoDTO.Tax_exchanged_info : null;

            var dbCmr = dbInv.Cmr ?? new InvoiceExtraInfoChecked();
            this.InvoiceExtraInfoCheckedMapper(dbCmr, infoDTO.Cmr);
            if (dbInv.Cmr == null)
            {
                dbCmr.CmrChecked = dbInv;
                this._db.Entry(dbCmr).State = EntityState.Added;
            }
            else
            {
                //delete if it was on database and now its unchecked
                if (infoDTO.Cmr.Checked == null || infoDTO.Cmr.Checked.Value == false)
                {
                    this._db.Entry(dbCmr).State = EntityState.Deleted;
                }
            }


            var dbRecived = dbInv.Recived ?? new InvoiceExtraInfoChecked();
            this.InvoiceExtraInfoCheckedMapper(dbRecived, infoDTO.Recived);
            if (dbInv.Recived == null)
            {
                dbRecived.RecivedChecked = dbInv;
                this._db.Entry(dbRecived).State = EntityState.Added;
            }
            else
            {
                //delete if it was on database and now its unchecked
                if (infoDTO.Recived == null || infoDTO.Recived.Checked.Value == false)
                {
                    this._db.Entry(dbRecived).State = EntityState.Deleted;
                }
            }

            var dbSent = dbInv.Sent ?? new InvoiceExtraInfoChecked();
            this.InvoiceExtraInfoCheckedMapper(dbSent, infoDTO.Sent);
            if (dbInv.Sent == null)
            {
                dbSent.SentChecked = dbInv;
                this._db.Entry(dbSent).State = EntityState.Added;
            }
            else
            {
                if (infoDTO.Sent == null || infoDTO.Sent.Checked.Value == false)
                {
                    this._db.Entry(dbSent).State = EntityState.Deleted;
                }
            }
        }

        public void InvoiceExtraInfoCheckedMapper(InvoiceExtraInfoChecked db, InvoiceExtraInfoCheckedDTO dto)
        {
            if (dto.Checked.HasValue && dto.Checked.Value)
            {
                db.Checked = true;
                db.Date = dto.Date.Value;
                db.Info = dto.Info;
            }
            else
            {
                db.Checked = false;
                db.Date = null;
                db.Info = null;
            }
        }

        public IQueryable<InvoiceBuy> InvoiceBuyQueryable()
        {
            return _db.InvoiceBuy
                    .Include(i => i.Load)
                    .Include(i => i.Currency)
                    .Include(i => i.InvoicePosList)
                    .Include(i => i.InvoiceTotal)
                    .Include(i => i.PaymentTerms).ThenInclude(i => i.PaymentTerm)
                    .Include(i => i.RatesValuesList)
                    .Include(i => i.Seller).ThenInclude(a => a.AddressList)
                    .Include(i => i.Seller).ThenInclude(e => e.EmployeeList)
                    .Include(i => i.Seller).ThenInclude(b => b.BankAccountList);
        }

        public IQueryable<InvoiceSell> InvoiceSellQueryable()
        {
            return this._db.InvoiceSell
                .Include(i => i.Load)
                .Include(i => i.Buyer).ThenInclude(i => i.AddressList)
                .Include(i => i.Buyer).ThenInclude(i => i.EmployeeList)
                .Include(i => i.Buyer).ThenInclude(i => i.BankAccountList)
                .Include(i => i.Currency)
                .Include(i => i.ExtraInfo)
                .Include(i => i.ExtraInfo).ThenInclude(i => i.Cmr)
                .Include(i => i.ExtraInfo).ThenInclude(i => i.Recived)
                .Include(i => i.ExtraInfo).ThenInclude(i => i.Sent)
                .Include(i => i.InvoicePosList)
                .Include(i => i.InvoiceTotal)
                .Include(i => i.PaymentTerms).ThenInclude(i => i.PaymentTerm)
                .Include(i => i.RatesValuesList)
                .Include(i => i.Seller).ThenInclude(i => i.AddressList)
                .Include(i => i.Seller).ThenInclude(i => i.EmployeeList)
                .Include(i => i.Seller).ThenInclude(i => i.BankAccountList)
                .Include(i => i.TransportOffer);

        }


        public async Task DeleteInvoiceBuy(int id, InvoiceBuy dbInv)
        {
            var db = dbInv ??  await this.InvoiceBuyQueryable()
                .FirstOrDefaultAsync(f => f.InvoiceBuyId == id);
            if (db == null) { return; }

            if (db.InvoicePosList.Count > 0)
            {
                foreach (var dbPos in db.InvoicePosList)
                {
                    this._db.Entry(dbPos).State = EntityState.Deleted;
                }
            }

            if (db.RatesValuesList.Count > 0)
            {
                foreach (var dbRate in db.RatesValuesList)
                {
                    this._db.Entry(dbRate).State = EntityState.Deleted;
                }
            }
            //this._db.Entry(db.Currency).State = EntityState.Deleted;
            //this._db.Entry(db.InvoiceTotal).State = EntityState.Deleted;
            //this._db.Entry(db.PaymentTerms).State = EntityState.Deleted;
            this._db.Entry(db).State = EntityState.Deleted;
        }

        public async Task DeleteInvoiceSell(int id)
        {
            var db = await this.InvoiceSellQueryable()
                .FirstOrDefaultAsync(f => f.InvoiceSellId == id);

            if (db == null) { return; }

            if (db.InvoicePosList.Count > 0)
            {
                foreach (var dbPos in db.InvoicePosList)
                {
                    this._db.Entry(dbPos).State = EntityState.Deleted;
                }
            }

            if (db.RatesValuesList.Count > 0)
            {
                foreach (var dbRate in db.RatesValuesList)
                {
                    this._db.Entry(dbRate).State = EntityState.Deleted;
                }
            }

            //var ei = db.ExtraInfo;
            //if (ei.Cmr != null) {
            //    this._db.Entry(ei.Cmr).State = EntityState.Deleted;
            //}
            //if (ei.Recived != null) {
            //    this._db.Entry(ei.Recived).State = EntityState.Deleted;
            //}
            //if (ei.Sent != null) {
            //    this._db.Entry(ei.Sent).State = EntityState.Deleted;
            //}
            //if (db.TransportOffer != null) {
            //    db.TransportOffer.InvoiceSellId = null;
            //}

            //this._db.Entry(db.ExtraInfo).State = EntityState.Deleted;
            //this._db.Entry(db.Currency).State = EntityState.Deleted;
            //this._db.Entry(db.InvoiceTotal).State = EntityState.Deleted;
            //this._db.Entry(db.PaymentTerms).State = EntityState.Deleted;
            
            this._db.Entry(db).State = EntityState.Deleted;
        }

        public InvoiceSellListDTO InvoiceSellDTOtoListDTO(InvoiceSellDTO dto)
        {
            var res = new InvoiceSellListDTO();

            res.Brutto = dto.Invoice_total.Total_brutto;
            res.DataSprzedazy = dto.Selling_date.ToString("yyyy-MM-dd");
            res.DocumentNo = dto.Invoice_no;
            res.Id = dto.Invoice_sell_id;
            res.Nabywca = dto.Buyer.Short_name;
            res.Netto = dto.Invoice_total.Total_netto;
            res.Podatek = dto.Invoice_total.Total_tax;


            if (dto.Extra_info.TransportOfferId.HasValue) {
                res.Type = "T";
            } else if (dto.Extra_info.LoadId.HasValue)
            {
                res.Type = "S";
            }
            else {
                res.Type = "";
            }


            res.Waluta = dto.Currency.Name;
            return res;
        }

        public InvoiceBuyListDTO InvoiceBuyDTOtoListDTO(InvoiceBuyDTO dto)
        {
            var res = new InvoiceBuyListDTO();
            res.Brutto = dto.Invoice_total.Total_brutto;
            res.DataSprzedazy = dto.Selling_date.ToString("yyyy-MM-dd");
            res.DocumentNo = dto.Invoice_no;
            res.Id = dto.Invoice_buy_id;
            res.Nabywca = dto.Seller.Short_name;
            res.Netto = dto.Invoice_total.Total_netto;
            res.Podatek = dto.Invoice_total.Total_tax;
            res.Waluta = dto.Currency.Name;

            return res;
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

        public void InvoiceRateMapper(RateValue rate, InvoiceRatesValuesDTO rateDTO)
        {
            rate.BruttoValue = rateDTO.Brutto_value;
            rate.NettoValue = rateDTO.Netto_value;
            rate.VatRate = rateDTO.Vat_rate;
            rate.VatValue = rateDTO.Vat_value;
        }

        public CurrencyDTO EtDTOCurrency(Currency curr)
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
            res.Currency = this.EtDTOCurrency(cNbp.Currency);
            res.Pln_value = cNbp.PlnValue;
            res.Price = cNbp.Price;
            res.Rate = cNbp.Rate;
            res.Rate_date = cNbp.RateDate;
            return res;
        }

        public InvoicePosDTO EtDTOInvoicePos(InvoicePos pos)
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


        public async Task<List<PaymentRequiredListDTO>> PaymentRequiredList()
        {
            //var invoicessToPay = await this._db.InvoiceSell
            //    .Where(w => w.ExtraInfo.InvoiceRecivedDate.HasValue && w.PaymentTerms.PaymentTerm.IsPaymentDate)
            //    .GroupBy(gDate => gDate.ExtraInfo.CmrRecivedDate.Value.AddDays(gDate.PaymentTerms.PaymentDays.Value))
            //    .Select(s => new {
            //        Date = s.Key,
            //        InvoiceList = s.ToList()
            //            .GroupBy(gCurr => gCurr.CurrencyId)
            //            .Select(sc => new {
            //                CurrencyId = sc.Key,
            //                InvoiceList = sc.ToList()
            //            }).ToList()
            //    })
            //    .ToListAsync();

            var dbRes = await this._db.InvoiceSell.ToListAsync();


            var res = new List<PaymentRequiredListDTO>();


            return res;
        }
    }
}
