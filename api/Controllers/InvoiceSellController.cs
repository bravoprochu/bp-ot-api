using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using bp.Pomocne.Linq;

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

            //var dbRes = await this._invoiceService.InvoiceSellQueryable()
            //    .Where(w=>w.SellingDate>=dateStart && w.SellingDate<=dateEnd)
            //    .OrderByDescending(o=>o.InvoiceSellId)
            //    .ToListAsync();

            var res = await this._GetAll(dateStart, dateEnd);

            return Ok(res);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var db = await this._GetById(id);
            if (db == null) { return NotFound(); }

            if (db.IsCorrection) {
                var org = await _GetById(db.BaseInvoiceId);
                if (org == null) { return NotFound(); }
                return Ok(this.EtoDTOInvoiceSellForInvoiceCorrection(db, org));
            }
            return Ok(db);
        }


        [HttpGet]
        public async Task<IActionResult> GetPaymentRemindList()
        {

            var dbRes = await this._invoiceService.InvoiceSellQueryable()
                .Where(w => w.IsInactive==false && w.PaymentIsDone == false)
                .ToListAsync();

            var dbCorrs = await this._invoiceService.InvoiceSellQueryable()
                .WhereIn(w => w.InvoiceSellId, dbRes.Where(wl => wl.BaseInvoiceId.HasValue).Select(sm => sm.BaseInvoiceId.Value).ToList())
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

                var dto = this.EtoDTOInvoiceSell(inv);
                if (inv.IsCorrection) {
                    dto = this.EtoDTOInvoiceSellForInvoiceCorrection(dto, EtoDTOInvoiceSell(dbCorrs.FirstOrDefault(f => f.InvoiceSellId == dto.BaseInvoiceId)));
                }

                var payToAdd = new InvoicePaymentRemindDTO();
                var rnd = new InvoicePaymentRemindDTO();

                rnd.Company = this._companyService.CompanyCardMapper(inv.Buyer);
                rnd.Currency = dto.Currency;
                rnd.InvoiceId = dto.InvoiceSellId;
                rnd.InvoiceNo = dto.IsCorrection? "Faktura korygująca: " + dto.InvoiceNo : "Faktura VAT: " + dto.InvoiceNo;
                rnd.InvoiceTotal = dto.InvoiceTotal.Current;
                rnd.InvoiceValue = dto.GetInvoiceValue;
                rnd.CorrectionPaymenntInfo = dto.GetCorrectionPaymenntInfo;
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
                },
                InvoiceValue = s.Sum(sv => sv.InvoiceValue)

            }).ToList();

            unpaidOverdueStats = unpaidOverdue.GroupBy(g => g.Currency.CurrencyId).Select(s => new InvoiceSellStatsDTO()
            {
                Currency = s.FirstOrDefault().Currency,
                Total = new InvoiceTotalDTO()
                {
                    Total_brutto = s.Sum(sum => sum.InvoiceTotal.Total_brutto),
                    Total_netto = s.Sum(sum => sum.InvoiceTotal.Total_netto),
                    Total_tax = s.Sum(sum => sum.InvoiceTotal.Total_tax)
                },
                InvoiceValue = s.Sum(sv => sv.InvoiceValue)
            }).ToList();

            notConfirmedStats = notConfirmed.GroupBy(g => g.Currency.CurrencyId).Select(s => new InvoiceSellStatsDTO()
            {
                Currency = s.FirstOrDefault().Currency,
                Total = new InvoiceTotalDTO()
                {
                    Total_brutto = s.Sum(sum => sum.InvoiceTotal.Total_brutto),
                    Total_netto = s.Sum(sum => sum.InvoiceTotal.Total_netto),
                    Total_tax = s.Sum(sum => sum.InvoiceTotal.Total_tax)
                },
                InvoiceValue = s.Sum(sv => sv.InvoiceValue)
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


        [HttpPost]
        public IActionResult PostCalcRates([FromBody] InvoiceSellDTO inv )
        {
            //var res = this._invoiceService.CalcRates()

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            

            return  Ok(inv);
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
                var newInvCorr = new InvoiceSell();
                CorrectionSetInActive(dbInvoice);
                
                newInvCorr.BaseInvoiceId = dbInvoice.InvoiceSellId;

                await this.InvoiceSellMapper(newInvCorr, dto);
                this._db.Entry(newInvCorr).State = EntityState.Added;
                try
                {
                    await this._db.SaveChangesAsync();
                    //setting new correctionID
                    dbInvoice.CorrectiondId = newInvCorr.InvoiceSellId;
                    await this._db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }

                var correctedDTO = this.EtoDTOInvoiceSellForInvoiceCorrection(this.EtoDTOInvoiceSell(newInvCorr), this.EtoDTOInvoiceSell(dbInvoice));
                return Ok(correctedDTO);
            }

            if (dto.IsCorrection && dto.InvoiceNo!=null)
            {

                if (dbInvoice.IsInactive == false)
                {
                    await this.InvoiceSellMapper(dbInvoice, dto);
                    await this._db.SaveChangesAsync();
                }

                var baseInvoice = await this._invoiceService.InvoiceSellQueryable()
                    .FirstOrDefaultAsync(f => f.InvoiceSellId == dbInvoice.BaseInvoiceId.Value);
                //return NoContent();
                return Ok(this.EtoDTOInvoiceSellForInvoiceCorrection(this.EtoDTOInvoiceSell(dbInvoice), this.EtoDTOInvoiceSell(baseInvoice)));
            }

            if (dbInvoice.IsInactive == false)
            {
                await this.InvoiceSellMapper(dbInvoice, dto);
            }
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
            db.InvoiceTotal.IsInactive = true;
        }


        private InvoiceSellDTO EtoDTOInvoiceSell(InvoiceSell inv)
        {
            var res = new InvoiceSellDTO();
            if (inv.BaseInvoiceId.HasValue) {
                res.BaseInvoiceId = inv.BaseInvoiceId.Value;
            }
            if (inv.CorrectiondId.HasValue) {
                res.CorrectionId = inv.CorrectiondId.Value;
            }
            res.IsCorrection = inv.IsCorrection;
            res.CompanyBuyer = _companyService.EtDTOCompany(inv.Buyer);
            res.CompanySeller = _companyService.EtDTOCompany(inv.Seller);
            this._invoiceService.EtDTOInvoiceCommon((inv), (InvoiceCommonDTO)res);
            res.ExtraInfo = (InvoiceExtraInfoDTO)this._invoiceService.EtoDTOExtraInfo(inv.ExtraInfo);
            if (inv.Load != null) {
                res.ExtraInfo.LoadId = inv.LoadId;
                res.ExtraInfo.LoadNo = inv.Load.LoadNo;
            }
            res.InvoiceSellId = inv.InvoiceSellId;

            if (inv.PaymentIsDone) {
                res.PaymentIsDone = true;
                res.PaymentDate = inv.PaymentDate;
            }
            res.PaymentTerms = _invoiceService.EtDTOPaymentTerms(inv.PaymentTerms);

            return res;
        }

        private InvoiceSellDTO EtoDTOInvoiceSellForInvoiceCorrection(InvoiceSellDTO corr, InvoiceSellDTO original)
        {
            var invoiceTypeName = original.IsCorrection ? "Faktura korygująca" : "Faktura VAT";
            corr.InvoiceOriginalNo = $"{invoiceTypeName} {original.InvoiceNo} z dnia {original.DateOfSell.ToShortDateString()}";
            corr.IsCorrection = true;
            //corr.Rates= original.Rates;
            corr.InvoiceTotal.Original = original.InvoiceTotal.Current;
            corr.InvoiceOriginalPaid = original.PaymentIsDone;
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

        private async Task<List<InvoiceSellListDTO>> _GetAll(DateTime dateStart, DateTime dateEnd)
        {
            var dbRes = await this._invoiceService.InvoiceSellQueryable()
                .Where(w => (w.SellingDate >= dateStart && w.SellingDate <= dateEnd) && w.IsInactive == false)
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
                else {
                    resList.Add(this.EtoDTOInvoiceSell(invoice));
                }
            }

            //corrections base list
            List<InvoiceSellDTO> corrOrgList = new List<InvoiceSellDTO>();
            var dbResOrg = await this._invoiceService.InvoiceSellQueryable()
                .WhereIn(w => w.InvoiceSellId, baseIds)
                .ToListAsync();

            if (dbResOrg.Count > 0) {
                foreach (var invOrg in dbResOrg)
                {
                    corrOrgList.Add(this.EtoDTOInvoiceSell(invOrg));
                }
            }

            //prep invsellList
            //corrections
            foreach (var inv in resCorrList)
            {
                    res.Add(this._invoiceService.InvoiceSellDTOtoListDTO(this.EtoDTOInvoiceSellForInvoiceCorrection(inv, corrOrgList.FirstOrDefault(f => f.InvoiceSellId == inv.BaseInvoiceId))));
            }
            //non corrections
            foreach (var inv in resList)
            {
                res.Add(this._invoiceService.InvoiceSellDTOtoListDTO(inv));
            }


            return res.OrderByDescending(o=>o.Id).ToList();
        }

        private async Task<InvoiceSellDTO> _GetById(int id)
        {
            var res= await this._invoiceService.InvoiceSellQueryable()
                .Where(w => w.InvoiceSellId == id)
                .FirstOrDefaultAsync();
            if (res == null) {return null;}

            return this.EtoDTOInvoiceSell(res);
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
            line.Corrections.Vat_unit_value = line.Current.Vat_unit_value - line.Original.Vat_unit_value;
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
                //rnd.InvoiceTotal = 
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

            this._invoiceService.InvoiceCommonMapper((InvoiceCommon)db, (InvoiceCommonDTO)dto, User, db);

            db.CorrectionTotalInfo = dto.GetCorrectionPaymenntInfo;
            db.DateOfIssue = dto.DateOfIssue;
            
            if (db.ExtraInfo == null) {
                db.ExtraInfo = new InvoiceExtraInfo();
                this._db.Entry(db.ExtraInfo).State = EntityState.Added;
            }
            this._invoiceService.InvoiceExtraInfoMapper(db.ExtraInfo, dto.ExtraInfo);

            db.IsCorrection = dto.IsCorrection;

            // override -- INVOICE_NO
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




        #endregion
    }
}

