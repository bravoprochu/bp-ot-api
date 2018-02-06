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

            var dbInvoice = await this._invoiceService.InvoiceSellQueryable()
                .FirstOrDefaultAsync(f=>f.InvoiceSellId==id);

            if (dbInvoice == null) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono faktury o Id: {id}"));
            }

            // diff buyerID
            if (dbInvoice.Buyer.CompanyId != dto.CompanyBuyer.CompanyId) {
                dbInvoice.Buyer = await this._db.Company.FindAsync(dto.CompanyBuyer.CompanyId);
            }
            if (dbInvoice.Currency.CurrencyId != dto.Currency.CurrencyId)
            {
                dbInvoice.Currency = this._invoiceService._currencyList.Where(w => w.CurrencyId == dto.Currency.CurrencyId).FirstOrDefault();
            }

            dbInvoice.DateOfIssue = dto.DateOfIssue;
            this._invoiceService.InvoiceExtraInfoMapper(dbInvoice.ExtraInfo, dto.ExtraInfo);
            dbInvoice.Info = dto.Info;

            //remove deleted pos
            foreach (var pos in dbInvoice.InvoicePosList)
            {
                if (!dto.InvoiceLines.Any(a => a.Current.Invoice_pos_id == pos.InvoicePosId)) {
                    this._db.Entry(pos).State = EntityState.Deleted;
                }
            }
            //modify or add pos
            foreach (var pos in dto.InvoiceLines)
            {
                var posDb = dbInvoice.InvoicePosList.Where(w => w.InvoicePosId== pos.Current.Invoice_pos_id).FirstOrDefault();
                if (posDb == null)
                {
                    var pDb = this._invoiceService.NewInvoicePosBasedOnDTOMapper(pos.Current);
                    pDb.InvoiceSell = dbInvoice;
                    this._db.Entry(pDb).State = EntityState.Added;
                }
                else {
                    this._invoiceService.InvoicePosMapperFromDTO(posDb, pos.Current);
                }
            }

            this._invoiceService.InvoiceTotalMapper(dbInvoice.InvoiceTotal, dto.InvoiceTotal.Current);
            this._invoiceService.PaymentTermsMapper(dbInvoice.PaymentTerms, dto.PaymentTerms);

            //remove rate value
            foreach (var rate in dbInvoice.RatesValuesList)
            {
                if(!dto.Rates_values_list.Any(a => a.Invoice_rates_values_id == rate.RateValueId)) {
                    this._db.Entry(rate).State = EntityState.Deleted;
                }
            }
            //modify or add rateValue
            foreach (var rate in dto.Rates_values_list)
            {
                var dbRate = dbInvoice.RatesValuesList.Where(w => w.RateValueId == rate.Invoice_rates_values_id).FirstOrDefault();
                if (dbRate == null)
                {
                    var newRate = this._invoiceService.NewInvoiceRateValueBasedOnDTOMapper(rate);
                    newRate.InvoiceSell = dbInvoice;
                    this._db.Entry(newRate).State = EntityState.Added;
                }
                else {
                    this._invoiceService.InvoiceTaxValueMapperFromDTO(dbRate, rate);
                }
            }
            if (dbInvoice.Seller.CompanyId != dto.CompanySeller.CompanyId) {
                dbInvoice.Seller = await _db.Company.FindAsync(dto.CompanySeller.CompanyId);
            }
            dbInvoice.SellingDate = dto.DateOfSell;
            if (dto.PaymentIsDone)
            {
                dbInvoice.PaymentIsDone = true;
                dbInvoice.PaymentDate = dto.PaymentDate;
            }
            else {
                dbInvoice.PaymentIsDone = false;
                dbInvoice.PaymentDate = null;
            }
         


            this._commonFunctions.CreationInfoUpdate((CreationInfo)dbInvoice, dto.CreationInfo, User);

            await this._db.SaveChangesAsync();

            return Ok(this.EtoDTOInvoiceSell(dbInvoice));
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
            
           
            foreach (var rate in dto.Rates_values_list)
            {

                var dbPos = this._invoiceService.NewInvoiceRateValueBasedOnDTOMapper(rate);
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

        private InvoiceSellDTO EtoDTOInvoiceSell(InvoiceSell inv)
        {
            var res = new InvoiceSellDTO();
            res.CreationInfo = new bp.Pomocne.CommonFunctions().CreationInfoMapper((CreationInfo)inv);
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
                    Current= this._invoiceService.EtDTOInvoicePos(invLine),
                    Original=new InvoiceLineDTO()

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

            res.Rates = new InvoiceRatesGroupDTO
            {
                Corrections = new List<InvoiceRatesValuesDTO>(),
                Current=new List<InvoiceRatesValuesDTO>(),
                Original=new List<InvoiceRatesValuesDTO>()
            };
            foreach (var rate in inv.RatesValuesList)
            {
                res.Rates.Current.Add(this._invoiceService.EtoDTORateValue(rate));
            }


            return res;
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
        




        #endregion
    }
}
