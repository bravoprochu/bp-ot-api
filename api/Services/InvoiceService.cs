using bp.jpkVat;
using bp.shared;
using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Models.Load;
using bp.shared.DocumentNumbers;
using bp.shared.DTO;
using bp.shared.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Dane.Invoice
{
    public partial class InvoiceService
    {
        private readonly BpKpirContextDane _db;
        public readonly List<Currency> _currencyList;
        private readonly CompanyService _companyService;

        public InvoiceService(BpKpirContextDane db, Company.CompanyService companyService)
        {
            this._db = db;
            this._currencyList = _db.Currency.ToList();
            this._companyService = companyService;
        }

        public string CurrencyPln => "PLN";


        public async Task<Company.Company> Owner () {
            return await this._db.Company.FirstOrDefaultAsync();
        }


        public void CalcInvoiceLineDTO(InvoiceLineDTO src) {
            double vatRateD = 0;
            double vatRateParsed = double.TryParse(src.Vat_rate, out vatRateD) ? double.Parse(src.Vat_rate) : 0;
            double vatRate= vatRateParsed > 0 ? vatRateParsed / 100 : 0;

            src.Vat_unit_value = Math.Round(src.Unit_price * vatRate, 2);
            src.Vat_value = Math.Round(src.Quantity * src.Vat_unit_value, 2);
            src.Netto_value = Math.Round(src.Quantity * src.Unit_price, 2);

            src.Brutto_value = Math.Round(src.Netto_value + src.Vat_value, 2);
        }
               
        
        public void EtoDTOCommon(InvoiceCommon db, InvoiceCommonDTO res)
        {
            if (db.CreatedDateTime.HasValue)
            {
                res.CreatedBy = db.CreatedBy;
                res.CreatedDateTime = db.CreatedDateTime.Value;
            }
            res.Currency = this.EtoDTOCurrency(db.Currency);
            res.DateOfIssue = db.DateOfIssue;
            res.DateOfSell = db.SellingDate;
            res.Info = db.Info;
            res.InvoiceLines = this.EtoDTOInvoiceLineGroup(db.InvoicePosList);
            res.InvoiceNo = db.InvoiceNo;
            res.ModifyBy = db.ModifyBy;
            if (db.ModifyDateTime.HasValue)
            {
                res.ModifyBy = db.ModifyBy;
                res.ModifyDateTime = db.ModifyDateTime.Value;
            }
            res.PaymentTerms = this.EtoDTOPaymentTerms(db.PaymentTerms);
        }
        public CurrencyDTO EtoDTOCurrency(Currency curr)
        {
            var res = new CurrencyDTO();
            res.CurrencyId = curr.CurrencyId;
            res.Description = curr.Description;
            res.Name = curr.Name;
            return res;
        }
        public CurrencyNbpDTO EtoDTOCurrencyNbp(CurrencyNbp cNbp)
        {
            var res = new CurrencyNbpDTO();
            res.Currency = this.EtoDTOCurrency(cNbp.Currency);
            res.PlnValue = cNbp.PlnValue;
            res.Price = cNbp.Price;
            res.Rate = cNbp.Rate;
            res.RateDate = cNbp.RateDate;
            return res;
        }
        public InvoiceLineDTO EtoDTOInvoiceLine(InvoicePos pos)
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
        private List<InvoiceLinesGroupDTO> EtoDTOInvoiceLineGroup(List<InvoicePos> db)
        {
            var res = new List<InvoiceLinesGroupDTO>();

            foreach (var line in db)
            {
                res.Add(new InvoiceLinesGroupDTO()
                {
                    //Corrections = new InvoiceLineDTO(),
                    Current = this.EtoDTOInvoiceLine(line),
                    Original = new InvoiceLineDTO()
                });
            }
            return res;
        }
        public PaymentTermsDTO EtoDTOPaymentTerms(PaymentTerms pTerms)
        {
            var res = new PaymentTermsDTO();
            res.Day0 = pTerms.Day0;
            if (pTerms.PaymentTerm.IsPaymentDate)
            {
                res.PaymentDate = pTerms.PaymentDate.Value;
                res.PaymentDays = pTerms.PaymentDays.Value;
            }
            res.Description = pTerms.PaymentTerm.IsDescription ? pTerms.PaymentDescription : null;
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
        public LoadExtraInfoDTO EtoDTOExtraInfo(InvoiceExtraInfo inv)
        {
            var res = new LoadExtraInfoDTO();
            res.CurrencyNbp = new CurrencyNbpDTO();
            res.Is_in_words = false;
            if (!string.IsNullOrWhiteSpace(inv.LoadNo))
            {
                res.Is_load_no = true;
                res.LoadNo = inv.LoadNo;

                if (inv.InvoiceSell.TransportOffer.CurrencyNbp.Currency.Name != "PLN") {
                    res.CurrencyNbp = EtoDTOCurrencyNbp(inv.InvoiceSell.TransportOffer.CurrencyNbp);
                    res.Is_tax_nbp_exchanged = true;

                }
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
        private InvoiceExtraInfoCheckedDTO EtoDTOExtraInfoChecked(InvoiceExtraInfoChecked db)
        {
            var res = new InvoiceExtraInfoCheckedDTO();
            res.InvoiceExtraInfoCheckedId = db.InvoiceExtraInfoCheckedId;
            res.Checked = db.Checked;
            res.Date = db.Date;
            res.Info = db.Info;

            return res;
        }
        public InvoiceBuyDTO EtoDTOInvoiceBuy(InvoiceBuy inv)
        {
            var res = new InvoiceBuyDTO();
            this.EtoDTOCommon((InvoiceCommon)inv, (InvoiceCommonDTO)res);
            res.InvoiceBuyId = inv.InvoiceBuyId;

            res.InvoiceReceivedDate = inv.InvoiceReceivedDate;
            res.IsInvoiceReceived = inv.InvoiceReceived;

            if (inv.InvoiceReceivedDate.HasValue)
            {
                res.IsInvoiceReceived = true;
                res.InvoiceReceivedDate = inv.InvoiceReceivedDate.Value;
            }
            else
            {
                res.InvoiceReceivedDate = null;
                res.IsInvoiceReceived = false;
            }

            if (inv.Load != null)
            {
                res.LoadId = inv.Load.LoadId;
                res.LoadNo = inv.Load.LoadNo;
            }
            if (inv.PaymentIsDone)
            {
                res.PaymentIsDone = true;
                res.PaymentDate = inv.PaymentDate;
            }
            else
            {
                res.PaymentIsDone = false;
                res.PaymentDate = null;
            }
            res.PaymentTerms = this.EtoDTOPaymentTerms(inv.PaymentTerms);
            res.CompanySeller = _companyService.EtDTOCompany(inv.CompanySeller);
            return res;
        }
        public InvoiceBuyListDTO EtoDTOInvoiceBuyList(InvoiceBuyDTO dto)
        {
            var res = new InvoiceBuyListDTO();
            res.Brutto = dto.InvoiceTotal.Current.Total_brutto;
            //res.DataSprzedazy = shared.DateHelp.DateHelpful.FormatDateToYYYYMMDD(dto.DateOfSell);
            res.DataSprzedazy = dto.DateOfSell;
            res.DocumentNo = dto.InvoiceNo;
            res.Id = dto.InvoiceBuyId;
            res.Nabywca = dto.CompanySeller.Short_name;
            res.Netto = dto.InvoiceTotal.Current.Total_netto;
            res.Podatek = dto.InvoiceTotal.Current.Total_tax;
            res.Waluta = dto.Currency.Name;

            return res;
        }
        public InvoiceSellDTO EtoDTOInvoiceSell(InvoiceSell inv)
        {
            var res = new InvoiceSellDTO();
            if (inv.BaseInvoiceId.HasValue)
            {
                res.BaseInvoiceId = inv.BaseInvoiceId.Value;
            }
            if (inv.CorrectiondId.HasValue)
            {
                res.CorrectionId = inv.CorrectiondId.Value;
            }
            res.IsCorrection = inv.IsCorrection;
            res.CompanyBuyer = _companyService.EtDTOCompany(inv.Buyer);
            res.CompanySeller = _companyService.EtDTOCompany(inv.Seller);
            this.EtoDTOCommon((inv), (InvoiceCommonDTO)res);
            res.ExtraInfo = (InvoiceExtraInfoDTO)this.EtoDTOExtraInfo(inv.ExtraInfo);
            if (inv.Load != null)
            {
                res.ExtraInfo.LoadId = inv.LoadId;
                res.ExtraInfo.LoadNo = inv.Load.LoadNo;
            }
            res.InvoiceSellId = inv.InvoiceSellId;

            if (inv.PaymentIsDone)
            {
                res.PaymentIsDone = true;
                res.PaymentDate = inv.PaymentDate;
            }
            res.PaymentTerms = this.EtoDTOPaymentTerms(inv.PaymentTerms);

            return res;
        }
        public InvoiceSellDTO EtoDTOInvoiceSellForInvoiceCorrection(InvoiceSellDTO corr, InvoiceSellDTO original)
        {
            var invoiceTypeName = original.IsCorrection ? "Faktura korygująca" : "Faktura VAT";
            corr.InvoiceOriginalNo = $"{invoiceTypeName} {original.InvoiceNo} z dnia {original.DateOfSell.ToShortDateString()}";
            corr.IsCorrection = true;
            //corr.Rates= original.Rates;
            corr.InvoiceTotal.Original = original.InvoiceTotal.Current;
            corr.InvoiceOriginalPaid = original.PaymentIsDone;
            if (original.ExtraInfo.TransportOfferId.HasValue)
            {
                corr.ExtraInfo.TransportOfferId = original.ExtraInfo.TransportOfferId.Value;
                corr.ExtraInfo.TransportOfferNo = original.ExtraInfo.TransportOfferNo;
            }
            if (original.ExtraInfo.LoadId.HasValue)
            {
                corr.ExtraInfo.LoadId = original.ExtraInfo.LoadId;
                corr.ExtraInfo.LoadNo = original.ExtraInfo.LoadNo;
            }
            foreach (var pos in corr.InvoiceLines)
            {
                pos.Original = original.InvoiceLines.Where(w => w.Current.Invoice_pos_id == pos.Current.BaseInvoiceLineId).Select(s => s.Current).FirstOrDefault();
                this.InvoiceLinesCorrecionsPrep(pos);
            }

            return corr;
        }
        private InvoiceSellListDTO EtoDTOInvoiceSellList(InvoiceSellDTO dto)
        {
            var res = new InvoiceSellListDTO();

            res.Brutto = dto.InvoiceTotal.Current.Total_brutto;
            //res.DataSprzedazy = bp.shared.DateHelp.DateHelpful.FormatDateToYYYYMMDD(dto.DateOfSell);
            var utcDate = dto.DateOfSell.ToLocalTime();
            res.DataSprzedazy = dto.DateOfSell;
            res.DocumentNo = dto.InvoiceNo;
            res.Id = dto.InvoiceSellId;
            res.Nabywca = dto.CompanyBuyer.Short_name;
            res.Netto = dto.InvoiceTotal.Current.Total_netto;
            res.Podatek = dto.InvoiceTotal.Current.Total_tax;

            var pos = string.Join("", dto.InvoiceLines.Select(s => s.Current).SelectMany(s => s.Name)).ToLower().Contains("najem");

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

        public async Task<string> GetNextInvoiceNo(DateTime invDate, string prefix = null)
        {
            var lastNo = await this._db.InvoiceSell.Where(w => w.IsCorrection == false && w.IsInactive == false).Select(s => s.InvoiceNo).LastOrDefaultAsync();

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

        private void InvoiceLinesCorrecionsPrep(InvoiceLinesGroupDTO line)
        {
            line.Corrections.Brutto_value = line.Current.Brutto_value - line.Original.Brutto_value;
            line.Corrections.Netto_value = line.Current.Netto_value - line.Original.Netto_value;
            line.Corrections.Vat_rate = line.Current.Vat_rate;
            line.Corrections.Vat_unit_value = line.Current.Vat_unit_value - line.Original.Vat_unit_value;
            line.Corrections.Vat_value = line.Current.Vat_value - line.Original.Vat_value;
            if (line.Current.Quantity != line.Original.Quantity)
            {
                line.Corrections.Quantity = line.Current.Quantity - line.Original.Quantity;
            }
            if (line.Current.Unit_price != line.Original.Unit_price)
            {
                line.Corrections.Unit_price = line.Current.Unit_price - line.Original.Unit_price;
            }
        }
        private void InvoicePosDelete(List<InvoicePos> db)
        {
            if (db.Count > 0)
            {
                foreach (var dbPos in db)
                {
                    this._db.Entry(dbPos).State = EntityState.Deleted;
                }
            }
        }
        private void InvoiceRatesDelete(List<RateValue> db)
        {
            if (db.Count > 0)
            {
                foreach (var dbPos in db)
                {
                    this._db.Entry(dbPos).State = EntityState.Deleted;
                }
            }
        }

        public async Task InvoiceBuyDelete(int id, InvoiceBuy dbInv)
        {
            var db = dbInv ?? await this.QueryableInvoiceBuy()
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

        public async Task<List<InvoiceBuyDTO>> InvoiceBuyGetAll(DateRangeDTO dateRange)
        {
            var dbRes= await this.QueryableInvoiceBuy()
                .Where(w => (w.SellingDate >= dateRange.DateStart && w.SellingDate <= dateRange.DateEnd))
                .OrderByDescending(o => o.InvoiceBuyId)
                .ToListAsync();
            var res = new List<InvoiceBuyDTO>();

            foreach (var inv in dbRes)
            {
                res.Add(this.EtoDTOInvoiceBuy(inv));
            }

            return res;
        }

        public async Task<List<InvoiceBuyListDTO>> InvoiceBuyGetAllToList(DateRangeDTO dateRange)
        {

            var dbResList = await this.QueryableInvoiceBuy()
            .Where(w => w.SellingDate >= dateRange.DateStart && w.SellingDate <= dateRange.DateEnd)
            .OrderByDescending(o => o.InvoiceBuyId)
            .ToListAsync();

            var res = new List<InvoiceBuyListDTO>();

            foreach (var inv in dbResList)
            {
                res.Add(this.EtoDTOInvoiceBuyList(this.EtoDTOInvoiceBuy(inv)));
            }
            return res;
        }
        public async Task<InvoiceBuyDTO> InvoiceBuyGetById(int id)
        {
            var res = await this.QueryableInvoiceBuy()
                .FirstOrDefaultAsync(f => f.InvoiceBuyId == id);

            if (res == null) { return null; }
            else
            {
                return this.EtoDTOInvoiceBuy(res);
            }
        }

        public string InvoiceNoTypeCorrection => "KOR";

        public async Task InvoiceSellDelete(int id)
        {
            var db = await this.QueryableInvoiceSell()
                .FirstOrDefaultAsync(f => f.InvoiceSellId == id);

            if (db == null) { return; }

            if (db.CorrectiondId.HasValue)
            {
                return;
            }

            var dbOrg = new InvoiceSell();

            if (db.BaseInvoiceId.HasValue)
            {
                dbOrg = await this.QueryableInvoiceSell()
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


            InvoicePosDelete(db.InvoicePosList);
            InvoiceRatesDelete(db.RatesValuesList);


            this._db.Entry(db).State = EntityState.Deleted;
        }
        public void InvoiceSellCorrectionSetInactive(InvoiceSell db)
        {
            db.IsInactive = true;
            if (db.InvoicePosList.Count > 0)
            {
                foreach (var pos in db.InvoicePosList)
                {
                    pos.IsInactive = true;
                }
            }

            if (db.RatesValuesList.Count > 0)
            {
                foreach (var rate in db.RatesValuesList)
                {
                    rate.IsInactive = true;
                }
            }
            db.InvoiceTotal.IsInactive = true;
        }
        public async Task<List<InvoiceSellListDTO>> InvoiceSellGetAllToList(DateRangeDTO dateRange)
        {
            var dbRes = await this.QueryableInvoiceSell()
                .Where(w => (w.SellingDate >= dateRange.DateStart && w.SellingDate <= dateRange.DateEnd) && w.IsInactive == false)
                .OrderByDescending(o => o.InvoiceSellId)
                .ToListAsync();

            List<InvoiceSellListDTO> res = new List<InvoiceSellListDTO>();
            List<InvoiceSellDTO> resList = new List<InvoiceSellDTO>();
            List<InvoiceSellDTO> resCorrList = new List<InvoiceSellDTO>();

            List<int> baseIds = new List<int>();

            foreach (var invoice in dbRes)
            {
                if (invoice.IsCorrection && invoice.BaseInvoiceId.HasValue)
                {
                    baseIds.Add(invoice.BaseInvoiceId.Value);
                    resCorrList.Add(this.EtoDTOInvoiceSell(invoice));
                }
                else
                {
                    resList.Add(this.EtoDTOInvoiceSell(invoice));
                }
            }

            //corrections base list
            List<InvoiceSellDTO> corrOrgList = new List<InvoiceSellDTO>();
            var dbResOrg = await this.QueryableInvoiceSell()
                .WhereIn(w => w.InvoiceSellId, baseIds)
                .ToListAsync();

            if (dbResOrg.Count > 0)
            {
                foreach (var invOrg in dbResOrg)
                {
                    corrOrgList.Add(this.EtoDTOInvoiceSell(invOrg));
                }
            }

            //prep invsellList
            //corrections
            foreach (var inv in resCorrList)
            {
                res.Add(this.EtoDTOInvoiceSellList(this.EtoDTOInvoiceSellForInvoiceCorrection(inv, corrOrgList.FirstOrDefault(f => f.InvoiceSellId == inv.BaseInvoiceId))));
            }
            //non corrections
            foreach (var inv in resList)
            {
                res.Add(this.EtoDTOInvoiceSellList(inv));
            }


            return res.OrderByDescending(o => o.Id).ToList();
        }
        public async Task<List<InvoiceSellDTO>> InvoiceSellGetAll(DateRangeDTO dateRange)
        {
            var res = new List<InvoiceSellDTO>();
            var dbRes= await this.QueryableInvoiceSell()
                .Where(w => (w.SellingDate >= dateRange.DateStart && w.SellingDate <= dateRange.DateEnd) && w.IsInactive == false)
                .OrderByDescending(o => o.InvoiceSellId)
                .ToListAsync();

            var dbOrgs = await this.QueryableInvoiceSell()
                .WhereIn(w => w.InvoiceSellId, dbRes.Where(w => w.BaseInvoiceId.HasValue).Select(s => s.BaseInvoiceId.Value).ToList())
                .ToListAsync();

            foreach (var inv in dbRes)
            {
                if (inv.IsCorrection && inv.BaseInvoiceId.HasValue)
                {
                    res.Add(this.EtoDTOInvoiceSellForInvoiceCorrection(this.EtoDTOInvoiceSell(inv), this.EtoDTOInvoiceSell(dbOrgs.Where(w => w.InvoiceSellId == inv.BaseInvoiceId.Value).FirstOrDefault())));
                }
                else {
                    res.Add(this.EtoDTOInvoiceSell(inv));
                }
            }

            return res;
        }
        public async Task<InvoiceSellDTO> InvoiceSellGetById(int id)
        {
            var res = await this.QueryableInvoiceSell()
                .Where(w => w.InvoiceSellId == id)
                .FirstOrDefaultAsync();
            if (res == null) { return null; }

            return this.EtoDTOInvoiceSell(res);
        }
        public async Task<List<InvoicePaymentRemindDTO>> InvoiceSellPaymentRemindList()
        {
            var dbRes = await this.QueryableInvoiceSell()
            .Include(i => i.Buyer.AddressList)
            .Where(w => w.PaymentIsDone == false)
            .ToListAsync();

            var res = new List<InvoicePaymentRemindDTO>();

            //foreach (var inv in dbRes)
            //{
            //    //var payToAdd = new InvoicePaymentRemindDTO();
            //    var rnda = new InvoicePaymentRemindDTO();
                

            //    rnda.Company = this._companyService.CompanyCardMapper(inv.Buyer);
            //    rnda.Currency = this.EtoDTOCurrency(inv.Currency);
            //    rnda.InvoiceId = inv.InvoiceSellId;
            //    rnda.InvoiceNo = inv.InvoiceNo;
            //    //rnd.InvoiceTotal = 
            //    //rnd.PaymentDate = inv.ExtraInfo.Recived.Date.Value.AddDays(inv.PaymentTerms.PaymentDays.Value);

            //    if (inv.PaymentTerms.PaymentDays.HasValue)
            //    {
            //        if ((inv.LoadId.HasValue) && (inv.ExtraInfo.Recived != null) && (inv.ExtraInfo.Recived.Date.HasValue))
            //        {
            //            rnda.PaymentDate = inv.ExtraInfo.Recived.Date.Value.AddDays(inv.PaymentTerms.PaymentDays.Value);
            //            res.Add(rnda);
            //        }
            //        if (!inv.LoadId.HasValue)
            //        {
            //            rnda.PaymentDate = inv.SellingDate.AddDays(inv.PaymentTerms.PaymentDays.Value);
            //            res.Add(rnda);
            //        }
            //    }
            //    else
            //    {
            //        rnda.PaymentDate = inv.SellingDate;
            //        res.Add(rnda);
            //    }
            //}
            return res;
        }

        public async Task<Jpk> GetJpk(DateRangeDTO dateRange, int celZlozenia=0)
        {
            var zakupy = await this.InvoiceBuyGetAll(dateRange);
            var sprzedaz = await this.InvoiceSellGetAll(dateRange);
            var podmiot = await this._companyService.GetCompanyById(1);

            var res = new Jpk();

            res.Naglowek.CelZlozenia = celZlozenia;
            res.Naglowek.DataOd = dateRange.DateStart;
            res.Naglowek.DataDo = dateRange.DateEnd;
            res.Naglowek.DataWytworzeniaJPK = DateTime.Now;
            res.Naglowek.KodFormularza = new JpkKodFormularza
            {
                KodSystemowy = "JPK_VAT (3)",
                WersjaSchemy = "1-1"
            };
            res.Naglowek.WariantFormularza = 3;

            res.Podmiot.Email = podmiot.Email;
            res.Podmiot.NIP = podmiot.Vat_id;
            res.Podmiot.PelnaNazwa = podmiot.Legal_name;

            //var stawkiSprzedaz = sprzedaz.SelectMany(sm => sm.Rates).GroupBy(g => g.Current.Vat_rate).Select(sg => new InvoiceRatesValuesDTO()
            //{
            //    Brutto_value=sg.Sum(s=>s.Current.Brutto_value),
            //    Netto_value=sg.Sum(s=>s.Current.Netto_value),

            //});
            
            int spCounter = 1;
            foreach (var sPos in sprzedaz)
            {
                bool isPln = sPos.Currency.Name == this.CurrencyPln;
                var wiersz = new JpkSprzedazWiersz();
                wiersz.LpSprzedazy = spCounter;
                wiersz.NrKontrahenta = sPos.CompanyBuyer.Vat_id;
                wiersz.NazwaKontrahenta = sPos.CompanyBuyer.Legal_name;
                wiersz.AdresKontrahenta = sPos.CompanyBuyer.AddressList.FirstOrDefault().AddressCombined;
                wiersz.DowodSprzedazy = sPos.InvoiceNo;
                wiersz.DataWystawienia = sPos.DateOfIssue;
                wiersz.DataSprzedazy = sPos.DateOfSell;

                //----------------------------------------
                //Kwota netto – Dostawa towarów oraz świadczenie usług na terytorium kraju, zwolnione od podatku(pole opcjonalne)
                var krajZwolniony = sPos.Rates.Where(f => f.VatRate.Contains("zw")).FirstOrDefault();
                if (isPln && krajZwolniony != null)
                {
                    wiersz.K_10 = krajZwolniony.Current.Netto_value;
                }

                //----------------------------------------
                //Kwota netto – Dostawa towarów oraz świadczenie usług na terytorium kraju, opodatkowane stawką 7% albo 8% oraz świadczenie usług taksówkowych opodatkowanych w formie ryczałtu 4% (pole opcjonalne)
                //Kwota podatku należnego – Dostawa towarów oraz świadczenie usług na terytorium kraju, opodatkowane stawką 7% albo 8% oraz świadczenie usług taksówkowych opodatkowanych w formie ryczałtu 4% (pole opcjonalne)
                var kraj8 = sPos.Rates.Where(w => w.VatRate == "7" || w.VatRate == "8" || w.VatRate == "4").FirstOrDefault();
                if (isPln && kraj8 != null) {
                    wiersz.K_17__K_18 = new JpkNettoPodatek
                    {
                        Netto = kraj8.Current.Netto_value,
                        Podatek = kraj8.Current.Vat_value
                    };
                }


                //----------------------------------------
                //Kwota netto – Dostawa towarów oraz świadczenie usług na terytorium kraju, opodatkowane stawką 22 % albo 23 % (pole opcjonalne)
                //Kwota podatku należnego – Dostawa towarów oraz świadczenie usług na terytorium kraju, opodatkowane stawką 22 % albo 23 % (pole opcjonalne)
                var kraj23 = sPos.Rates.Where(w => w.VatRate.Contains("23")).FirstOrDefault();
                if (isPln && kraj23 != null)
                {
                    wiersz.K_19__K_20 = new JpkNettoPodatek
                    {
                        Netto = kraj23.Current.Netto_value,
                        Podatek = kraj23.Current.Vat_value
                    };
                }

              

                if (isPln)
                {
                    res.Sprzedaz.SprzedazWiersz.Add(wiersz);
                    spCounter++;
                }
            }

            //zakupy
            int zakupyCounter = 1;
            foreach (var zPos in zakupy)
            {
                
                var wiersz = new JpkZakupWiersz();
                wiersz.LpZakupu = zakupyCounter;
                wiersz.AdresDostawcy = zPos.CompanySeller.AddressList.FirstOrDefault().AddressCombined;
                wiersz.DataZakupu = zPos.DateOfSell;
                wiersz.DowodZakupu = zPos.InvoiceNo;
                wiersz.NazwaDostawcy = zPos.CompanySeller.Legal_name;
                wiersz.NrDostawcy = zPos.CompanySeller.Vat_id;


                //---K_43__K_44-------------------------------------
                //Kwota netto/podatek – Nabycie towarów i usług zaliczanych u podatnika do środków trwałych(pole opcjonalne)



                //---K_45__K_46-------------------------------------
                //Kwota netto – Nabycie towarów i usług pozostałych(pole opcjonalne)
                wiersz.K_45__K_46.Netto = zPos.InvoiceTotal.Current.Total_netto;
                wiersz.K_45__K_46.Podatek = zPos.InvoiceTotal.Current.Total_tax;

                //---K_47-------------------------------------------
                //Korekta podatku naliczonego od nabycia środków trwałych(pole opcjonalne)


                //---K_48-------------------------------------------
                //Korekta podatku naliczonego od pozostałych nabyć (pole opcjonalne)


                //---K_49-------------------------------------------
                //Korekta podatku naliczonego, o której mowa w art. 89b ust. 1 ustawy (pole opcjonalne)


                //---K_50/-------------------------------------------
                //Korekta podatku naliczonego, o której mowa w art. 89b ust. 4 ustawy (pole opcjonalne)

                if (zPos.InvoiceReceivedDate.HasValue) {
                    wiersz.DataWplywu = zPos.InvoiceReceivedDate.Value;
                    res.Zakup.ZakupWiersz.Add(wiersz);
                    zakupyCounter++;
                };
                
            }
            return res;
        }


        
        public void MapperCurrencyNb(CurrencyNbp dbCur, CurrencyNbpDTO curDTO)
        {
            dbCur.Currency = this._currencyList.Find(f => f.CurrencyId == curDTO.Currency.CurrencyId);
            dbCur.PlnValue = curDTO.PlnValue;
            dbCur.Price = curDTO.Price;
            dbCur.Rate = curDTO.Rate;
            dbCur.RateDate = curDTO.RateDate;
        }
        public void MapperCommon(InvoiceCommon db, InvoiceCommonDTO dto, ClaimsPrincipal user, InvoiceBuy invBuy)
        {
            new bp.shared.CommonFunctions().CreationInfoUpdate((CreationInfo)db, (CreationInfo)dto, user);
            db.Currency = this._currencyList.FirstOrDefault(f => f.CurrencyId == dto.Currency.CurrencyId);
            db.DateOfIssue = dto.DateOfIssue;
            db.Info = dto.Info;
            db.InvoiceNo = dto.InvoiceNo;
            this.MapperLineGroup(invBuy, dto);
            //invoiceTotal
            if (db.InvoiceTotal == null)
            {
                db.InvoiceTotal = new InvoiceTotal();
                this._db.Entry(db.InvoiceTotal).State = EntityState.Added;
            }
            this.MapperTotal(db.InvoiceTotal, dto.InvoiceTotal.Current);

            //payment Terms
            if (db.PaymentTerms == null)
            {
                db.PaymentTerms = new PaymentTerms();
                this._db.Entry(db.PaymentTerms).State = EntityState.Added;
            }
            this.MapperPaymentTerms(db.PaymentTerms, dto.PaymentTerms);

            this.MapperRatesGroup(db, dto, invBuy);
        }
        public void MapperCommon(InvoiceCommon db, InvoiceCommonDTO dto, ClaimsPrincipal user, InvoiceSell invSell)
        {
            new bp.shared.CommonFunctions().CreationInfoUpdate((CreationInfo)db, (CreationInfo)dto, user);
            db.Currency = this._currencyList.FirstOrDefault(f => f.CurrencyId == dto.Currency.CurrencyId);
            db.DateOfIssue = dto.DateOfIssue;
            db.Info = dto.Info;
            this.MapperLineGroup(invSell, dto);
            //invoiceTotal
            if (db.InvoiceTotal == null)
            {
                db.InvoiceTotal = new InvoiceTotal();
                this._db.Entry(db.InvoiceTotal).State = EntityState.Added;
            }
            this.MapperTotal(db.InvoiceTotal, dto.InvoiceTotal.Current);

            //payment Terms
            if (db.PaymentTerms == null)
            {
                db.PaymentTerms = new PaymentTerms();
                this._db.Entry(db.PaymentTerms).State = EntityState.Added;
            }
            this.MapperPaymentTerms(db.PaymentTerms, dto.PaymentTerms);

            this.MapperRatesGroup(db, dto, invSell);
        }
        private void MapperExtraInfo(InvoiceExtraInfo db, InvoiceExtraInfoDTO dto)
        {
            db.LoadNo = dto.Is_load_no ? dto.LoadNo : null;
            db.TaxExchangedInfo = dto.Is_tax_nbp_exchanged ? dto.Tax_exchanged_info : null;

            var dbCmr = db.Cmr ?? new InvoiceExtraInfoChecked();
            this.MapperExtraInfoChecked(dbCmr, dto.Cmr);
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
            this.MapperExtraInfoChecked(dbRecived, dto.Recived);
            if (db.Recived == null)
            {
                dbRecived.RecivedChecked = db;
                this._db.Entry(dbRecived).State = EntityState.Added;
            }
            else
            {
                //delete if it was on database and now its unchecked
                if (dto.Recived == null || (dto.Recived.Checked.HasValue && dto.Recived.Checked.Value == false))
                {
                    this._db.Entry(dbRecived).State = EntityState.Deleted;
                }
            }

            var dbSent = db.Sent ?? new InvoiceExtraInfoChecked();
            this.MapperExtraInfoChecked(dbSent, dto.Sent);
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
        private void MapperExtraInfoChecked(InvoiceExtraInfoChecked db, InvoiceExtraInfoCheckedDTO dto)
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
        private void MapperRate(RateValue rate, InvoiceRatesValuesDTO rateDTO)
        {
            rate.BruttoValue = rateDTO.Brutto_value;
            rate.NettoValue = rateDTO.Netto_value;
            rate.VatRate = rateDTO.Vat_rate;
            rate.VatValue = rateDTO.Vat_value;
        }
        public void MapperLine(InvoicePos dbPos, InvoiceLineDTO posDTO)
        {
            if (posDTO.BaseInvoiceLineId.HasValue && posDTO.BaseInvoiceLineId.Value > 0)
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
        private void MapperLineGroup(InvoiceSell db, InvoiceCommonDTO dto)
        {
            //posLIST
            //remove deleted pos
            if (db.InvoicePosList == null)
            {
                db.InvoicePosList = new List<InvoicePos>();
            }
            foreach (var pos in db.InvoicePosList)
            {
                if (!dto.InvoiceLines.Any(a => a.Current.Invoice_pos_id == pos.InvoicePosId))
                {
                    this._db.Entry(pos).State = EntityState.Deleted;
                }
            }
            //modify or add pos
            foreach (var pos in dto.InvoiceLines)
            {
                var posDb = db.InvoicePosList.Where(w => w.InvoicePosId == pos.Current.Invoice_pos_id).FirstOrDefault();
                if (posDb == null)
                {

                    var pDb = new InvoicePos();
                    this.MapperLine(pDb, pos.Current);
                    pDb.InvoiceSell = db;
                    this._db.Entry(pDb).State = EntityState.Added;
                }
                else
                {
                    this.MapperLine(posDb, pos.Current);
                }
            }
        }
        private void MapperLineGroup(InvoiceBuy db, InvoiceCommonDTO dto)
        {
            //posLIST
            //remove deleted pos
            if (db.InvoicePosList == null)
            {
                db.InvoicePosList = new List<InvoicePos>();
            }
            foreach (var pos in db.InvoicePosList)
            {
                if (!dto.InvoiceLines.Any(a => a.Current.Invoice_pos_id == pos.InvoicePosId))
                {
                    this._db.Entry(pos).State = EntityState.Deleted;
                }
            }
            //modify or add pos
            foreach (var pos in dto.InvoiceLines)
            {
                var posDb = db.InvoicePosList.Where(w => w.InvoicePosId == pos.Current.Invoice_pos_id).FirstOrDefault();
                if (posDb == null)
                {

                    var pDb = new InvoicePos();
                    this.MapperLine(pDb, pos.Current);
                    pDb.InvoiceBuy = db;
                    this._db.Entry(pDb).State = EntityState.Added;
                }
                else
                {
                    this.MapperLine(posDb, pos.Current);
                }
            }
        }
        private void MapperRatesGroup(InvoiceCommon db, InvoiceCommonDTO dto, InvoiceSell invSell)
        {
            //remove rate value
            if (db.RatesValuesList == null)
            {
                db.RatesValuesList = new List<RateValue>();
            }
            foreach (var rate in db.RatesValuesList)
            {
                if (!dto.Rates.Any(a => a.Current.Invoice_rates_values_id == rate.RateValueId))
                {
                    this._db.Entry(rate).State = EntityState.Deleted;
                }
            }
            //modify or add rateValue
            foreach (var rate in dto.Rates)
            {
                var dbRate = db.RatesValuesList.Where(w => w.RateValueId == rate.Current.Invoice_rates_values_id).FirstOrDefault();
                if (dbRate == null)
                {
                    var newRate = new RateValue();
                    this.MapperRate(newRate, rate.Current);
                    newRate.InvoiceSell = invSell;
                    this._db.Entry(newRate).State = EntityState.Added;
                }
                else
                {
                    this.MapperRate(dbRate, rate.Current);
                }
            }
        }
        private void MapperRatesGroup(InvoiceCommon db, InvoiceCommonDTO dto, InvoiceBuy invBuy)
        {
            //remove rate value
            if (db.RatesValuesList == null)
            {
                db.RatesValuesList = new List<RateValue>();
            }
            foreach (var rate in db.RatesValuesList)
            {
                if (!dto.Rates.Any(a => a.Current.Invoice_rates_values_id == rate.RateValueId))
                {
                    this._db.Entry(rate).State = EntityState.Deleted;
                }
            }
            //modify or add rateValue
            foreach (var rate in dto.Rates)
            {
                var dbRate = db.RatesValuesList.Where(w => w.RateValueId == rate.Current.Invoice_rates_values_id).FirstOrDefault();
                if (dbRate == null)
                {
                    var newRate = new RateValue();
                    this.MapperRate(newRate, rate.Current);
                    newRate.InvoiceBuy = invBuy;
                    this._db.Entry(newRate).State = EntityState.Added;
                }
                else
                {
                    this.MapperRate(dbRate, rate.Current);
                }
            }
        }
        public void MapperPaymentTerms(PaymentTerms dbTerms, PaymentTermsDTO pDTO)
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
        public async Task MapperInvoiceBuy(InvoiceBuy dbInvoice, InvoiceBuyDTO dto, ClaimsPrincipal user)
        {
            this.MapperCommon((InvoiceCommon)dbInvoice, (InvoiceCommonDTO)dto, user, dbInvoice);
            dbInvoice.CompanySeller = await this._companyService.CompanyMapper(dbInvoice.CompanySeller, dto.CompanySeller);
            if (dto.PaymentIsDone)
            {
                dbInvoice.PaymentIsDone = true;
                dbInvoice.PaymentDate = dto.PaymentDate;
            }
            else
            {
                dbInvoice.PaymentIsDone = false;
                dbInvoice.PaymentDate = null;
            }

            //invoiceReceived
            if (dto.IsInvoiceReceived.HasValue && dto.IsInvoiceReceived.Value == true)
            {
                dbInvoice.InvoiceReceived = true;
                dbInvoice.InvoiceReceivedDate = dto.InvoiceReceivedDate.Value;
            }
            else
            {
                dbInvoice.InvoiceReceived = false;
                dbInvoice.InvoiceReceivedDate = null;
            }

            //if theres no load ref invoiceRecived default is true;
            if (dbInvoice.Load == null)
            {
                dbInvoice.InvoiceReceived = true;
            }
            else
            {
                dbInvoice.InvoiceReceived = dto.IsInvoiceReceived.Value;
            }

            dbInvoice.SellingDate = dto.DateOfSell;
        }
        public async Task MapperInvoiceSell(InvoiceSell db, InvoiceSellDTO dto, ClaimsPrincipal user)
        {
            db.Buyer = await this._companyService.CompanyMapper(db.Buyer, dto.CompanyBuyer);
            db.Seller = await this._companyService.Owner();

            this.MapperCommon((InvoiceCommon)db, (InvoiceCommonDTO)dto, user, db);

            db.CorrectionTotalInfo = dto.GetCorrectionPaymenntInfo;
            db.DateOfIssue = dto.DateOfIssue;

            if (db.ExtraInfo == null)
            {
                db.ExtraInfo = new InvoiceExtraInfo();
                this._db.Entry(db.ExtraInfo).State = EntityState.Added;
            }
            this.MapperExtraInfo(db.ExtraInfo, dto.ExtraInfo);

            db.IsCorrection = dto.IsCorrection;

            // override -- INVOICE_NO
            if (dto.IsCorrection)
            {
                if (dto.InvoiceNo == null)
                {
                    //assign new invCorrNo
                    db.InvoiceNo = await this.GetNextInvoiceCorrectionNo(dto.DateOfSell);
                }
            }
            if (dto.InvoiceSellId == 0)
            {
                db.InvoiceNo = await this.GetNextInvoiceNo(dto.DateOfSell);
            }
            db.SellingDate = dto.DateOfSell;


            if (dto.PaymentIsDone)
            {
                db.PaymentIsDone = true;
                db.PaymentDate = dto.PaymentDate;
            }
            else
            {
                db.PaymentIsDone = false;
                db.PaymentDate = null;
            }

            if (dto.ExtraInfo.TransportOfferId.HasValue)
            {
                db.TransportOfferId = dto.ExtraInfo.TransportOfferId.Value;
            }

        }
        private void MapperTotal(InvoiceTotal dbInv, InvoiceTotalDTO invDTO)
        {
            dbInv.TotalBrutto = invDTO.Total_brutto;
            dbInv.TotalNetto = invDTO.Total_netto;
            dbInv.TotalTax = invDTO.Total_tax;
        }

        public IQueryable<InvoiceBuy> QueryableInvoiceBuy()
        {
            return _db.InvoiceBuy
                    .Include(i => i.Load)
                    .Include(i => i.Currency)
                    .Include(i => i.InvoicePosList)
                    .Include(i => i.InvoiceTotal)
                    .Include(i => i.PaymentTerms).ThenInclude(i => i.PaymentTerm)
                    .Include(i => i.RatesValuesList)
                    .Include(i => i.CompanySeller).ThenInclude(a => a.AddressList)
                    .Include(i => i.CompanySeller).ThenInclude(e => e.EmployeeList)
                    .Include(i => i.CompanySeller).ThenInclude(b => b.BankAccountList);
        }
        public IQueryable<InvoiceSell> QueryableInvoiceSell()
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
                .Include(i => i.TransportOffer)
                .Include(i => i.TransportOffer).ThenInclude(i => i.CurrencyNbp)
                .Include(i => i.TransportOffer).ThenInclude(i => i.CurrencyNbp).ThenInclude(i => i.Currency);

        }
    }
}
