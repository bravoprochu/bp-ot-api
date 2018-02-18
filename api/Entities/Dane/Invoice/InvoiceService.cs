using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Models.Load;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bp.ot.s.API.Entities.Dane.Company;
using bp.Pomocne.DocumentNumbers;

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
            dbCur.Currency = this._currencyList.Find(f => f.CurrencyId == dbCur.CurrencyId);
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
            res.InvoiceSellNo = inv.InvoiceSell?.InvoiceNo;

            if (inv.InvoiceSell?.TransportOffer != null)
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

        public void InvoiceExtraInfoMapper(InvoiceExtraInfo db, InvoiceExtraInfoDTO dto)
        {
            db.LoadNo = dto.Is_load_no ? dto.LoadNo : null;
            db.TaxExchangedInfo = dto.Is_tax_nbp_exchanged ? dto.Tax_exchanged_info : null;

            var dbCmr = db.Cmr ?? new InvoiceExtraInfoChecked();
            this.InvoiceExtraInfoCheckedMapper(dbCmr, dto.Cmr);
            if (db.Cmr == null)
            {
                dbCmr.CmrChecked = db;
                this._db.Entry(dbCmr).State = EntityState.Added;
            }
            else
            {
                //delete if it was on database and now its unchecked
                if (dto.Cmr.Checked == null || dto?.Cmr.Checked.Value == false)
                {
                    this._db.Entry(dbCmr).State = EntityState.Deleted;
                }
            }


            var dbRecived = db.Recived ?? new InvoiceExtraInfoChecked();
            this.InvoiceExtraInfoCheckedMapper(dbRecived, dto.Recived);
            if (db.Recived == null)
            {
                dbRecived.RecivedChecked = db;
                this._db.Entry(dbRecived).State = EntityState.Added;
            }
            else
            {
                //delete if it was on database and now its unchecked
                if (dto.Recived == null || (dto.Recived.Checked.HasValue && dto.Recived.Checked.Value==false))
                {
                    this._db.Entry(dbRecived).State = EntityState.Deleted;
                }
            }

            var dbSent = db.Sent ?? new InvoiceExtraInfoChecked();
            this.InvoiceExtraInfoCheckedMapper(dbSent, dto.Sent);
            if (db.Sent == null)
            {
                dbSent.SentChecked = db;
                this._db.Entry(dbSent).State = EntityState.Added;
            }
            else
            {
                if (dto.Sent == null || (dto.Sent.Checked.HasValue && dto.Sent.Checked.Value == false))
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

        public void DeleteInvoicePos(List<InvoicePos> db) {
            if (db.Count > 0)
            {
                foreach (var dbPos in db)
                {
                    this._db.Entry(dbPos).State = EntityState.Deleted;
                }
            }
        }

        public void DeleteInvoiceRates(List<RateValue> db)
        {
            if (db.Count > 0)
            {
                foreach (var dbPos in db)
                {
                    this._db.Entry(dbPos).State = EntityState.Deleted;
                }
            }
        }


        public async Task DeleteInvoiceSell(int id)
        {
            var db = await this.InvoiceSellQueryable()
                .FirstOrDefaultAsync(f => f.InvoiceSellId == id);

            if (db == null) { return; }

            if (db.CorrectiondId.HasValue) {
                return;
            }

            var dbOrg = new InvoiceSell();

            if (db.BaseInvoiceId.HasValue)
            {
                dbOrg = await this.InvoiceSellQueryable()
                    .FirstOrDefaultAsync(f => f.InvoiceSellId == db.BaseInvoiceId.Value);
                if (dbOrg == null)
                {
                    return;
                }
                //setting back active to inactive pos (corrected invoice)
                foreach (var pos in dbOrg.InvoicePosList)
                {
                    pos.IsInactive = false;
                }
                //setting back active to inactive rates (corrected invoice)
                foreach (var pos in dbOrg.InvoicePosList)
                {
                    pos.IsInactive = false;
                }
                dbOrg.CorrectiondId = null;
                dbOrg.InvoiceTotal.IsInactive = false;
                dbOrg.IsInactive = false;
            }


            DeleteInvoicePos(db.InvoicePosList);
            DeleteInvoiceRates(db.RatesValuesList);

            
            this._db.Entry(db).State = EntityState.Deleted;
        }

        public async Task<string> GetNextInvoiceNo(DateTime invDate, string prefix = null)
        {
            var lastNo = await this._db.InvoiceSell.Where(w => w.IsCorrection == false && w.IsInactive==false).Select(s => s.InvoiceNo).LastOrDefaultAsync();

            if (lastNo == null)
            {
                var docNo = new DocNumber().GenNumberMonthYearNumber(null, invDate);
                docNo.Prefix = prefix;
                return docNo.DocNumberCombined;
            }
            else
            {
                return new DocNumber().GenNumberMonthYearNumber(lastNo, invDate).DocNumberCombined;
            }
        }

        public async Task<string> GetNextInvoiceCorrectionNo(DateTime invDate)
        {
            var lastNo = await this._db.InvoiceSell.Where(w => w.IsCorrection == true && w.IsInactive == false).Select(s => s.InvoiceNo).LastOrDefaultAsync();

            if (lastNo == null)
            {
                var docNo = new DocNumber().GenNumberMonthYearNumber(null, invDate);
                docNo.Prefix = this.InvoiceNoTypeCorrection;
                return docNo.DocNumberCombined;
            }
            else
            {
                return new DocNumber().GenNumberMonthYearNumber(lastNo, invDate).DocNumberCombined;
            }
        }





        public InvoiceSellListDTO InvoiceSellDTOtoListDTO(InvoiceSellDTO dto)
        {
            var res = new InvoiceSellListDTO();

            res.Brutto = dto.InvoiceTotal.Current.Total_brutto;
            res.DataSprzedazy = bp.Pomocne.DateHelp.DateHelpful.DateFormatYYYYMMDD(dto.DateOfSell);
            res.DocumentNo = dto.InvoiceNo;
            res.Id = dto.InvoiceSellId;
            res.Nabywca = dto.CompanyBuyer.Short_name;
            res.Netto = dto.InvoiceTotal.Current.Total_netto;
            res.Podatek = dto.InvoiceTotal.Current.Total_tax;

            var pos = string.Join("", dto.InvoiceLines.Select(s=>s.Current).SelectMany(s => s.Name)).ToLower().Contains("najem");

            if (dto.ExtraInfo.TransportOfferId.HasValue)
            {
                res.Type = "TRANS";
            }
            else if (dto.ExtraInfo.LoadId.HasValue)
            {
                res.Type = "SPED";
            }
            else if (pos)
            {
                res.Type = "NAJEM";
            }
            else
            {
                res.Type = null;
            }

        

            res.Waluta = dto.Currency.Name;
            return res;
        }

        public InvoiceBuyListDTO InvoiceBuyDTOtoListDTO(InvoiceBuyDTO dto)
        {
            var res = new InvoiceBuyListDTO();
            res.Brutto = dto.InvoiceTotal.Total_brutto;
            res.DataSprzedazy = Pomocne.DateHelp.DateHelpful.DateFormatYYYYMMDD(dto.dateOfSell);
            res.DocumentNo = dto.InvoiceNo;
            res.Id = dto.Invoice_buy_id;
            res.Nabywca = dto.Seller.Short_name;
            res.Netto = dto.InvoiceTotal.Total_netto;
            res.Podatek = dto.InvoiceTotal.Total_tax;
            res.Waluta = dto.Currency.Name;

            return res;
        }

        public void InvoiceTotalMapper(InvoiceTotal dbInv, InvoiceTotalDTO invDTO)
        {
            dbInv.TotalBrutto = invDTO.Total_brutto;
            dbInv.TotalNetto = invDTO.Total_netto;
            dbInv.TotalTax = invDTO.Total_tax;
        }

        public InvoicePos NewInvoicePosBasedOnDTOMapper(InvoiceLineDTO dto)
        {
            var pos = new InvoicePos();

            pos.BruttoValue = dto.Brutto_value;
            pos.BaseInvoiceLineId = dto.BaseInvoiceLineId.HasValue ? dto.BaseInvoiceLineId : null;
            pos.CorrectionInfo = dto.CorrectionInfo;
            pos.IsCorrected = dto.IsCorrected;
            pos.MeasurementUnit = dto.Measurement_unit;
            pos.Name = dto.Name;
            pos.NettoValue = dto.Netto_value;
            pos.Pkwiu = dto.Pkwiu;
            pos.Quantity = dto.Quantity;
            pos.UnitPrice = dto.Unit_price;
            pos.VatRate = dto.Vat_rate;
            pos.VatUnitValue = dto.Vat_unit_value;
            pos.VatValue = dto.Vat_value;
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

        public InvoiceLineDTO EtDTOInvoiceLine(InvoicePos pos)
        {
            var res = new InvoiceLineDTO();
            if (pos.BaseInvoiceLineId.HasValue)
            {
                res.BaseInvoiceLineId = pos.BaseInvoiceLineId.Value;
            }
            res.Brutto_value = pos.BruttoValue;
            res.Invoice_pos_id = pos.InvoicePosId;
            res.CorrectionInfo = pos.CorrectionInfo;
            res.IsCorrected = pos.IsCorrected;
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

        public string InvoiceNoTypeCorrection => "KOR";

        public void InvoiceLineMapper(InvoicePos dbPos, InvoiceLineDTO posDTO)
        {
            if (posDTO.BaseInvoiceLineId.HasValue && posDTO.BaseInvoiceLineId.Value>0)
            {
                dbPos.BaseInvoiceLineId = posDTO.BaseInvoiceLineId.Value;
            }
            dbPos.BruttoValue = posDTO.Brutto_value;
            dbPos.CorrectionInfo = posDTO.CorrectionInfo;
            dbPos.IsCorrected = posDTO.IsCorrected;
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

        //public void InvoiceTaxValueMapperFromDTO(RateValue dbRate, InvoiceRatesValuesDTO rateDTO)
        //{
        //    dbRate.BruttoValue = rateDTO.Brutto_value;
        //    dbRate.NettoValue = rateDTO.Netto_value;
        //    dbRate.VatRate = rateDTO.Vat_rate;
        //    dbRate.VatValue = rateDTO.Vat_value;
        //}
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
