﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bp.PomocneLocal.Pdf;
using System.IO;
using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Context;
using Microsoft.EntityFrameworkCore;
using bp.Pomocne.DocumentNumbers;
using bp.ot.s.API.Models.Load;
using System.Data.Common;
using bp.Pomocne.DTO;
using bp.Pomocne;
using bp.ot.s.API.Entities.Dane.Invoice;

namespace bp.ot.s.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Finanse")]
    public class InvoiceSellController : Controller
    {
        private readonly OfferTransDbContextDane _db;
        private readonly PdfRaports _pdf;
        private readonly CompanyService _companyService;
        private InvoiceService _invoiceService;
        private CommonFunctions _commonFunctions;

        public InvoiceSellController(OfferTransDbContextDane db, PdfRaports pdf, CompanyService companyService, InvoiceService invoiceService, CommonFunctions commonFunctions)
        {
            this._db = db;
            this._pdf = pdf;
            this._companyService = companyService;
            this._invoiceService = invoiceService;
            this._commonFunctions = commonFunctions;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await this._invoiceService.DeleteInvoiceSell(id);


            await this._db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{dateStart}/{dateEnd}")]
        public async Task<IActionResult> GetAll(DateTime dateStart, DateTime dateEnd)
        {
            dateEnd = bp.Pomocne.DateHelp.DateHelpful.DateRangeDateTo(dateEnd);

            var dbRes = await this._invoiceService.InvoiceSellQueryable()
                .Where(w=>w.SellingDate>=dateStart && w.SellingDate<=dateEnd)
                .OrderByDescending(o=>o.InvoiceSellId)
                .ToListAsync();

            List<InvoiceSellListDTO> resList = new List<InvoiceSellListDTO>();
            foreach (var invoice in dbRes)
            {
                resList.Add(this._invoiceService.InvoiceSellDTOtoListDTO(this.EtoDTOInvoiceSell(invoice)));
            }

            return Ok(resList);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dbRes= await this._invoiceService.InvoiceSellQueryable()
                .Where(w => w.InvoiceSellId == id)
                .FirstOrDefaultAsync();

            if (dbRes == null) {
                return NotFound();
            }

            if (dbRes.IsCorrection)
            {
                var original = await this._invoiceService.InvoiceSellQueryable()
                    .Where(w => w.InvoiceSellId == dbRes.BaseInvoiceId.Value)
                    .FirstOrDefaultAsync();
                if (original != null)
                {
                    return Ok(this.EtoDTOInvoiceSellCorrection(this.EtoDTOInvoiceSell(dbRes), this.EtoDTOInvoiceSell(original)));
                }
                else
                {
                    return NotFound();
                }
            }
            

            return Ok(this.EtoDTOInvoiceSell(dbRes));
        }


        [HttpGet]
        public async Task<IActionResult> GetPaymentRemindList()
        {

            var dbRes = await this._invoiceService.InvoiceSellQueryable()
                .Where(w => w.PaymentIsDone == false)
                .ToListAsync();

            var unpaid = new List<InvoicePaymentRemindDTO>();
            var unpaidStats = new List<InvoiceSellStatsDTO>();
            var unpaidOverdue = new List<InvoicePaymentRemindDTO>();
            var unpaidOverdueStats = new List<InvoiceSellStatsDTO>();
            var notConfirmed = new List<InvoicePaymentRemindDTO>();
            var notConfirmedStats = new List<InvoiceSellStatsDTO>();


            //Unpaid; every transport invoice with RECIVED date and every not load invoice;
            foreach (var inv in dbRes)
            {
                var payToAdd = new InvoicePaymentRemindDTO();
                var rnd = new InvoicePaymentRemindDTO();

                rnd.Company = this._companyService.CompanyCardMapper(inv.Buyer);
                rnd.Currency = this._invoiceService.EtDTOCurrency(inv.Currency);
                rnd.InvoiceId = inv.InvoiceSellId;
                rnd.InvoiceNo = inv.InvoiceNo;
                rnd.InvoiceTotal = this._invoiceService.EtoDTOInvoiceTotal(inv.InvoiceTotal);
                //rnd.PaymentDate = inv.ExtraInfo.Recived.Date.Value.AddDays(inv.PaymentTerms.PaymentDays.Value);

                if (inv.PaymentTerms.PaymentDays.HasValue)
                {
                    if (inv.LoadId.HasValue)
                    {
                        if (inv.ExtraInfo.Recived != null && inv.ExtraInfo.Recived.Date.HasValue)
                        {
                            //only confirmed invoice - RECIVED
                            rnd.PaymentDate = inv.ExtraInfo.Recived.Date.Value.AddDays(inv.PaymentTerms.PaymentDays.Value);
                            if (rnd.PaymentDate < DateTime.Now)
                            {
                                unpaidOverdue.Add(rnd);
                            }
                            else {
                                unpaid.Add(rnd);
                            }
                        }
                        else {
                            //not confirmed recived
                            rnd.PaymentDate = inv.SellingDate.AddDays(inv.PaymentTerms.PaymentDays.Value);
                            notConfirmed.Add(rnd);
                        }
                    } else
                    {
                        rnd.PaymentDate = inv.SellingDate.AddDays(inv.PaymentTerms.PaymentDays.Value);
                        if (rnd.PaymentDate < DateTime.Now)
                        {
                            unpaidOverdue.Add(rnd);
                        }
                        else
                        {
                            unpaid.Add(rnd);
                        }
                    }
                }
                else
                {
                    rnd.PaymentDate = inv.SellingDate;
                    if (rnd.PaymentDate < DateTime.Now)
                    {
                        unpaidOverdue.Add(rnd);
                    }
                    else
                    {
                        unpaid.Add(rnd);
                    }
                }
            }

            unpaidStats = unpaid.GroupBy(g => g.Currency.CurrencyId).Select(s => new InvoiceSellStatsDTO()
            {
                Currency = s.FirstOrDefault().Currency,
                Total = new InvoiceTotalDTO()
                {
                    Total_brutto = s.Sum(sum => sum.InvoiceTotal.Total_brutto),
                    Total_netto = s.Sum(sum => sum.InvoiceTotal.Total_netto),
                    Total_tax = s.Sum(sum => sum.InvoiceTotal.Total_tax)
                }
            }).ToList();
            unpaidOverdueStats = unpaidOverdue.GroupBy(g => g.Currency.CurrencyId).Select(s => new InvoiceSellStatsDTO()
            {
                Currency = s.FirstOrDefault().Currency,
                Total = new InvoiceTotalDTO()
                {
                    Total_brutto = s.Sum(sum => sum.InvoiceTotal.Total_brutto),
                    Total_netto = s.Sum(sum => sum.InvoiceTotal.Total_netto),
                    Total_tax = s.Sum(sum => sum.InvoiceTotal.Total_tax)
                }
            }).ToList();
            notConfirmedStats = notConfirmed.GroupBy(g => g.Currency.CurrencyId).Select(s => new InvoiceSellStatsDTO()
            {
                Currency = s.FirstOrDefault().Currency,
                Total = new InvoiceTotalDTO()
                {
                    Total_brutto = s.Sum(sum => sum.InvoiceTotal.Total_brutto),
                    Total_netto = s.Sum(sum => sum.InvoiceTotal.Total_netto),
                    Total_tax = s.Sum(sum => sum.InvoiceTotal.Total_tax)
                }
            }).ToList();



            var res = new
            {
                Unpaid = unpaid.OrderBy(o => o.PaymentDate).ToList(),
                UnpaidStats = unpaidStats,
                UnpaidOverdue= unpaidOverdue.OrderBy(o => o.PaymentDate).ToList(),
                UnpaidOverdueStats=unpaidOverdueStats,
                NotConfirmed = notConfirmed.OrderBy(o => o.PaymentDate).ToList(),
                NotConfirmedStats = notConfirmedStats
            };

          

            return Ok(res);
        }


        [HttpGet]
        public async Task<IActionResult> GetPaymentRemindByDateList()
        {
            var payments = await this.InvoiceSellPaymentRemindList();
            var res = payments.GroupBy(g => g.PaymentDate)
                .Select(s => new
                {
                    PaymentDay = s.Key,
                    Payments = s.ToList()
                });


            return Ok(res);
        }

        [HttpGet("{id}/{paymentDate}")]
        public async Task<IActionResult> PaymentConfirmation(int id, DateTime paymentDate)
        {
            var inv = await this._db.InvoiceSell.FirstOrDefaultAsync(f => f.InvoiceSellId == id);
            //inv.Info += $" Zapłacono {paymentDate.ToShortDateString()}";
            inv.PaymentDate = paymentDate;
            inv.PaymentIsDone = true;

            await this._db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] InvoiceSellDTO dto)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbInvoice = new InvoiceSell();
            if (id > 0)
            {
                dbInvoice = await this._invoiceService.InvoiceSellQueryable()
                    .FirstOrDefaultAsync(f => f.InvoiceSellId == id);

                if (dbInvoice == null)
                {
                    return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono faktury o Id: {id}"));
                }
            }
            else {
                this._db.Entry(dbInvoice).State = EntityState.Added;
            }

            //correction - create NEW 
            if (dto.IsCorrection && dto.InvoiceNo==null)
            {
                //setting inActive
                CorrectionSetInActive(dbInvoice);
                var newInvCorr = new InvoiceSell();

                //newInvCorr.InvoiceNo = await _invoiceService.GetNextInvoiceCorrectionNo(dto.DateOfSell);
                newInvCorr.BaseInvoiceId = dbInvoice.InvoiceSellId;
                //newInvCorr.IsCorrection = true;

                await this.InvoiceSellMapper(newInvCorr, dto);
                this._db.Entry(newInvCorr).State = EntityState.Added;
                try
                {
                    await this._db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }

                var correctedDTO = this.EtoDTOInvoiceSellCorrection(this.EtoDTOInvoiceSell(newInvCorr), this.EtoDTOInvoiceSell(dbInvoice));
                return Ok(correctedDTO);
            }

            if (dto.IsCorrection && dto.InvoiceNo!=null)
            {
                await this.InvoiceSellMapper(dbInvoice, dto);
                //this._commonFunctions.CreationInfoUpdate((CreationInfo)dbInvoice, dto.CreationInfo, User);

                await this._db.SaveChangesAsync();
                return NoContent();
            }

            await this.InvoiceSellMapper(dbInvoice, dto);
            //this._commonFunctions.CreationInfoUpdate((CreationInfo)dbInvoice, dto.CreationInfo, User);
            try
            {
                await this._db.SaveChangesAsync();
            }
            catch(Exception e)
            {
                throw e;
            }


            return Ok(EtoDTOInvoiceSell(dbInvoice));
        }

        [HttpPost]
        public async  Task<IActionResult> Post([FromBody] InvoiceSellDTO dto)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbInvoice = new InvoiceSell();
            dbInvoice.Buyer = this._companyService.GetCompanyById(dto.CompanyBuyer.CompanyId.Value);
            dbInvoice.Currency = this._invoiceService._currencyList.Where(w => w.CurrencyId == dto.Currency.CurrencyId).FirstOrDefault();
            dbInvoice.DateOfIssue = dto.DateOfIssue;
            


            var extraInfo = new InvoiceExtraInfo();
            this._invoiceService.InvoiceExtraInfoMapper(extraInfo, dto.ExtraInfo);
            extraInfo.InvoiceSell = dbInvoice;
            this._db.Entry(extraInfo).State = EntityState.Added;

            dbInvoice.Info = dto.Info;
            dbInvoice.InvoiceNo = new DocNumber().GenNumberMonthYearNumber(this._db.InvoiceSell.LastOrDefault()?.InvoiceNo, dto.DateOfSell).DocNumberCombined;

            foreach (var pos in dto.InvoiceLines)
            {
                var dbPos = this._invoiceService.NewInvoicePosBasedOnDTOMapper(pos.Current);
                dbPos.InvoiceSell = dbInvoice;
                this._db.Entry(dbPos).State = EntityState.Added;
            }

            var invTotal = new InvoiceTotal();
            this._invoiceService.InvoiceTotalMapper(invTotal, dto.InvoiceTotal.Current);
            invTotal.InvoiceSell = dbInvoice;
            this._db.Entry(invTotal).State = EntityState.Added;


            var payTerms = new PaymentTerms();
            _invoiceService.PaymentTermsMapper(payTerms, dto.PaymentTerms);
            payTerms.InvoiceSell = dbInvoice;
            this._db.Entry(payTerms).State = EntityState.Added;
            
           
            foreach (var rate in dto.Rates)
            {

                var dbPos = this._invoiceService.NewInvoiceRateValueBasedOnDTOMapper(rate.Current);
                dbPos.InvoiceSell = dbInvoice;
                this._db.Entry(dbPos).State = EntityState.Added;
            }

            dbInvoice.Seller = this._companyService.GetCompanyById(dto.CompanySeller.CompanyId.Value);
            dbInvoice.SellingDate = dto.DateOfSell;
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
            this._commonFunctions.CreationInfoUpdate((CreationInfo)dbInvoice, dto.CreationInfo, User);

            this._db.Entry(dbInvoice).State = EntityState.Added;


            await this._db.SaveChangesAsync();

            return Ok(this.EtoDTOInvoiceSell(dbInvoice));
        }

        [HttpPost]
        public IActionResult GenInvoicePdf([FromBody] InvoiceSellDTO invoiceSell)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            MemoryStream ms = new MemoryStream(_pdf.InvoicePdf(invoiceSell).ToArray());
            return File(ms, "application/pdf", "invoice.pdf");
        }

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var dbRes = await this._invoiceService.InvoiceSellQueryable()
        //        .Where(w => w.InvoiceSellId == id).FirstOrDefaultAsync();

        //    if (dbRes == null) { return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Delete", $"Nie znaleziono faktury o ID: {id} ")); }


        //    foreach (var rate in dbRes.RatesValuesList)
        //    {
        //        this._db.Entry(rate).State = EntityState.Deleted;
        //    }

        //    foreach (var pos in dbRes.InvoicePosList)
        //    {
        //        this._db.Entry(pos).State = EntityState.Deleted;
        //    }

        //    this._db.Entry(dbRes).State = EntityState.Deleted;

        //    try
        //    {
        //        await this._db.SaveChangesAsync();
        //    }
        //    catch (DbException)
        //    {

        //        throw;
        //    }

            
            

        //    return Ok("Usunięto, wszystko ok");
        //}


        
        
        #region private helpers

        private void BasicDTOtoEntityMapping(InvoiceSell dbInvoice, InvoiceSellDTO invoiceDTO)
        {
            dbInvoice.DateOfIssue = invoiceDTO.DateOfIssue;
            dbInvoice.Info = invoiceDTO.Info;
            dbInvoice.SellingDate = invoiceDTO.DateOfSell;
        }


        private void CorrectionSetInActive(InvoiceSell db)
        {
            db.IsInactive = true;
            if (db.InvoicePosList.Count > 0) {
                foreach (var pos in db.InvoicePosList)
                {
                    pos.IsInactive = true;
                }
            }

            if (db.RatesValuesList.Count > 0) {
                foreach (var rate in db.RatesValuesList)
                {
                    rate.IsInactive = true;
                }
            }
        }


        private InvoiceSellDTO EtoDTOInvoiceSell(InvoiceSell inv)
        {
            var res = new InvoiceSellDTO();
            if (inv.BaseInvoiceId.HasValue) {
                res.BaseInvoiceId = inv.BaseInvoiceId.Value;
            }
            res.CorrectionTotalInfo = inv.CorrectionTotalInfo;

            res.CreationInfo= new bp.Pomocne.CommonFunctions().EtDTOCreationInfoMapper(inv);
            res.CompanyBuyer = _companyService.EtDTOCompany(inv.Buyer);
            res.CompanySeller = _companyService.EtDTOCompany(inv.Seller);
            res.Currency = this._invoiceService.EtDTOCurrency(inv.Currency);
            res.DateOfIssue = inv.DateOfIssue;
            res.DateOfSell = inv.SellingDate;
            res.ExtraInfo = (InvoiceExtraInfoDTO)this._invoiceService.EtoDTOExtraInfo(inv.ExtraInfo);
            if (inv.Load != null) {
                res.ExtraInfo.LoadId = inv.LoadId;
                res.ExtraInfo.LoadNo = inv.Load.LoadNo;
            }
            res.Info = inv.Info;
            res.InvoiceNo = inv.InvoiceNo;

            foreach (var invLine in inv.InvoicePosList)
            {
                res.InvoiceLines.Add(new InvoiceLinesGroupDTO
                {
                    Corrections=new InvoiceLineDTO(),
                    Current= this._invoiceService.EtDTOInvoiceLine(invLine),
                    Original= this._invoiceService.EtDTOInvoiceLine(invLine)
                });
            }

            res.InvoiceSellId = inv.InvoiceSellId;
            

            res.InvoiceTotal = new InvoiceTotalGroupDTO {
                Corrections = res.IsCorrection ? this._invoiceService.EtoDTOInvoiceTotal(inv.InvoiceTotal) : new InvoiceTotalDTO(),
                Current = this._invoiceService.EtoDTOInvoiceTotal(inv.InvoiceTotal),
                Original=res.IsCorrection? this._invoiceService.EtoDTOInvoiceTotal(inv.InvoiceTotal): new InvoiceTotalDTO()
            };

            if (inv.PaymentIsDone) {
                res.PaymentIsDone = true;
                res.PaymentDate = inv.PaymentDate;
            }
            res.PaymentTerms = _invoiceService.EtDTOPaymentTerms(inv.PaymentTerms);

            res.Rates = new List<InvoiceRatesGroupDTO>();

            foreach (var rate in inv.RatesValuesList)
            {
                var newRateGroup = new InvoiceRatesGroupDTO();
                newRateGroup.VatRate = rate.VatRate;
                newRateGroup.Current = this._invoiceService.EtoDTORateValue(rate);
                newRateGroup.Original = this._invoiceService.EtoDTORateValue(rate);

                res.Rates.Add(newRateGroup);
            }


            return res;
        }


        private InvoiceSellDTO EtoDTOInvoiceSellCorrection(InvoiceSellDTO corr, InvoiceSellDTO original)
        {
            
            corr.InvoiceOriginalNo = original.InvoiceNo;
            corr.IsCorrection = true;
            corr.Rates= original.Rates;
            corr.InvoiceTotal.Original = original.InvoiceTotal.Current;
            corr.InvoiceOriginalNo = original.InvoiceNo;
            corr.invoiceOriginalPaid = original.PaymentIsDone;
            if (original.ExtraInfo.TransportOfferId.HasValue) {
                corr.ExtraInfo.TransportOfferId = original.ExtraInfo.TransportOfferId.Value;
                corr.ExtraInfo.TransportOfferNo = original.ExtraInfo.TransportOfferNo;
            }
            if (original.ExtraInfo.LoadId.HasValue) {
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


        private async Task<List<InvoicePaymentRemindDTO>> InvoiceSellPaymentRemindTransportNoConfirmationList()
        {
            var dbRes = await this._invoiceService.InvoiceSellQueryable()
            .Include(i => i.Buyer.AddressList)
            .Where(w => w.PaymentIsDone == false)
            .ToListAsync();

            var res = new List<InvoicePaymentRemindDTO>();

            return res;
        }

        private void InvoiceLinesCorrecionsPrep(InvoiceLinesGroupDTO line)
        {
            line.Corrections.Brutto_value = line.Current.Brutto_value - line.Original.Brutto_value;
            line.Corrections.Netto_value = line.Current.Netto_value - line.Original.Netto_value;
            line.Corrections.Vat_rate = line.Current.Vat_rate;
            line.Corrections.Vat_value = line.Current.Vat_value - line.Original.Vat_value;
            if (line.Current.Quantity != line.Original.Quantity) {
                line.Corrections.Quantity = line.Current.Quantity - line.Original.Quantity;
            }
            if (line.Current.Unit_price != line.Original.Unit_price) {
                line.Corrections.Unit_price = line.Current.Unit_price - line.Original.Unit_price;
            }
        }

        private async Task<List<InvoicePaymentRemindDTO>> InvoiceSellPaymentRemindList()
        {
            var dbRes = await this._invoiceService.InvoiceSellQueryable()
            .Include(i => i.Buyer.AddressList)
            .Where(w=>w.PaymentIsDone==false)
            .ToListAsync();

            var res = new List<InvoicePaymentRemindDTO>();

            foreach (var inv in dbRes)
            {
                var payToAdd = new InvoicePaymentRemindDTO();
                var rnd = new InvoicePaymentRemindDTO();

                rnd.Company = this._companyService.CompanyCardMapper(inv.Buyer);
                rnd.Currency = this._invoiceService.EtDTOCurrency(inv.Currency);
                rnd.InvoiceId = inv.InvoiceSellId;
                rnd.InvoiceNo = inv.InvoiceNo;
                rnd.InvoiceTotal = this._invoiceService.EtoDTOInvoiceTotal(inv.InvoiceTotal);
                //rnd.PaymentDate = inv.ExtraInfo.Recived.Date.Value.AddDays(inv.PaymentTerms.PaymentDays.Value);

                if (inv.PaymentTerms.PaymentDays.HasValue)
                {
                    if ((inv.LoadId.HasValue) && (inv.ExtraInfo.Recived!=null) && (inv.ExtraInfo.Recived.Date.HasValue))
                    {
                        rnd.PaymentDate = inv.ExtraInfo.Recived.Date.Value.AddDays(inv.PaymentTerms.PaymentDays.Value);
                        res.Add(rnd);
                    }
                    if (!inv.LoadId.HasValue)
                    {
                        rnd.PaymentDate = inv.SellingDate.AddDays(inv.PaymentTerms.PaymentDays.Value);
                        res.Add(rnd);
                    }
                }
                else {
                    rnd.PaymentDate = inv.SellingDate;
                    res.Add(rnd);
                }
            }
            return res;
        }


        private async Task InvoiceSellMapper(InvoiceSell db, InvoiceSellDTO dto)
        {
            db.Buyer= await this._companyService.CompanyMapper(db.Buyer, dto.CompanyBuyer);
            db.Seller=await this._companyService.CompanyMapper(db.Seller, dto.CompanySeller);

            db.CorrectionTotalInfo = dto.CorrectionTotalInfo;
            this._commonFunctions.CreationInfoUpdate((CreationInfo)db, dto.CreationInfo, User);
            db.CorrectionTotalInfo = dto.CorrectionTotalInfo;

            db.Currency = this._invoiceService._currencyList.Find(f => f.CurrencyId == dto.Currency.CurrencyId);

            db.CorrectionTotalInfo = dto.CorrectionTotalInfo;
            db.DateOfIssue = dto.DateOfIssue;

            if (db.ExtraInfo == null) {
                db.ExtraInfo = new InvoiceExtraInfo();
                this._db.Entry(db.ExtraInfo).State = EntityState.Added;
            }
            this._invoiceService.InvoiceExtraInfoMapper(db.ExtraInfo, dto.ExtraInfo);
            db.Info = dto.Info;
            db.IsCorrection = dto.IsCorrection;

            //INVOICE_NO
            if (dto.IsCorrection) {
                if (dto.InvoiceNo == null) {
                    //assign new invCorrNo
                    db.InvoiceNo = await this._invoiceService.GetNextInvoiceCorrectionNo(dto.DateOfSell);
                }
            }
            if (dto.InvoiceSellId == 0)
            {
                db.InvoiceNo = await this._invoiceService.GetNextInvoiceNo(dto.DateOfSell);
            }

            //posLIST
            //remove deleted pos
            if (db.InvoicePosList == null) {
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
                    this._invoiceService.InvoiceLineMapper(pDb, pos.Current);
                    pDb.InvoiceSell = db;
                    this._db.Entry(pDb).State = EntityState.Added;
                }
                else
                {
                    this._invoiceService.InvoiceLineMapper(posDb, pos.Current);
                }
            }

            db.SellingDate = dto.DateOfSell;

            var total = db.InvoiceTotal == null ? new InvoiceTotal() : db.InvoiceTotal;
            this._invoiceService.InvoiceTotalMapper(total, dto.InvoiceTotal.Current);
            if (db.InvoiceTotal == null) {
                db.InvoiceTotal = total;
                this._db.Entry(total).State = EntityState.Added;
            }

            var terms = db.PaymentTerms == null ? new PaymentTerms() : db.PaymentTerms;
            this._invoiceService.PaymentTermsMapper(terms, dto.PaymentTerms);
            if (db.PaymentTerms == null) {
                db.PaymentTerms = terms;
                this._db.Entry(terms).State = EntityState.Added;
            }

            //remove rate value
            if (db.RatesValuesList == null) {
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
                    var newRate = this._invoiceService.NewInvoiceRateValueBasedOnDTOMapper(rate.Current);
                    newRate.InvoiceSell = db;
                    this._db.Entry(newRate).State = EntityState.Added;
                }
                else
                {
                    this._invoiceService.InvoiceRateMapper(dbRate, rate.Current);
                }
            }
            
            
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




        #endregion
    }
}
