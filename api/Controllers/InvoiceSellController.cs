﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bp.PomocneLocal.Pdf;
using bp.ot.s.API.Entities.Dane.Invoice;
using System.IO;
using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Context;
using Microsoft.EntityFrameworkCore;
using bp.Pomocne.DocumentNumbers;
using bp.ot.s.API.Models.Load;
using System.Data.Common;
using bp.Pomocne.DTO;
using bp.Pomocne;

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
        public async Task<IActionResult> GetPaymentRequiredList()
        {
            var res = await this._invoiceService.PaymentRequiredList();

            return Ok(res);
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
            if (dbInvoice.Buyer.CompanyId != dto.Buyer.CompanyId) {
                dbInvoice.Buyer = await this._db.Company.FindAsync(dto.Buyer.CompanyId);
            }
            if (dbInvoice.Currency.CurrencyId != dto.Currency.CurrencyId)
            {
                dbInvoice.Currency = this._invoiceService._currencyList.Where(w => w.CurrencyId == dto.Currency.CurrencyId).FirstOrDefault();
            }

            dbInvoice.DateOfIssue = dto.Date_of_issue;
            this._invoiceService.InvoiceExtraInfoMapper(dbInvoice.ExtraInfo, dto.Extra_info);
            dbInvoice.Info = dto.Info;

            //remove deleted pos
            foreach (var pos in dbInvoice.InvoicePosList)
            {
                if (!dto.Invoice_pos_list.Any(a => a.Invoice_pos_id == pos.InvoicePosId)) {
                    this._db.Entry(pos).State = EntityState.Deleted;
                }
            }
            //modify or add pos
            foreach (var pos in dto.Invoice_pos_list)
            {
                var posDb = dbInvoice.InvoicePosList.Where(w => w.InvoicePosId== pos.Invoice_pos_id).FirstOrDefault();
                if (posDb == null)
                {
                    var pDb = this._invoiceService.NewInvoicePosBasedOnDTOMapper(pos);
                    pDb.InvoiceSell = dbInvoice;
                    this._db.Entry(pDb).State = EntityState.Added;
                }
                else {
                    this._invoiceService.InvoicePosMapperFromDTO(posDb, pos);
                }
            }

            this._invoiceService.InvoiceTotalMapper(dbInvoice.InvoiceTotal, dto.Invoice_total);
            this._invoiceService.PaymentTermsMapper(dbInvoice.PaymentTerms, dto.Payment_terms);

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
            if (dbInvoice.Seller.CompanyId != dto.Seller.CompanyId) {
                dbInvoice.Seller = await _db.Company.FindAsync(dto.Seller.CompanyId);
            }
            dbInvoice.SellingDate = dto.Selling_date;

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
            dbInvoice.Buyer = this._companyService.GetCompanyById(dto.Buyer.CompanyId.Value);
            dbInvoice.Currency = this._invoiceService._currencyList.Where(w => w.CurrencyId == dto.Currency.CurrencyId).FirstOrDefault();
            dbInvoice.DateOfIssue = dto.Date_of_issue;
            


            var extraInfo = new InvoiceExtraInfo();
            this._invoiceService.InvoiceExtraInfoMapper(extraInfo, dto.Extra_info);
            extraInfo.InvoiceSell = dbInvoice;
            this._db.Entry(extraInfo).State = EntityState.Added;

            dbInvoice.Info = dto.Info;
            dbInvoice.InvoiceNo = new DocNumber().GenNumberMonthYearNumber(this._db.InvoiceSell.LastOrDefault()?.InvoiceNo, dto.Selling_date).DocNumberCombined;

            foreach (var pos in dto.Invoice_pos_list)
            {
                var dbPos = this._invoiceService.NewInvoicePosBasedOnDTOMapper(pos);
                dbPos.InvoiceSell = dbInvoice;
                this._db.Entry(dbPos).State = EntityState.Added;
            }

            var invTotal = new InvoiceTotal();
            this._invoiceService.InvoiceTotalMapper(invTotal, dto.Invoice_total);
            invTotal.InvoiceSell = dbInvoice;
            this._db.Entry(invTotal).State = EntityState.Added;


            var payTerms = new PaymentTerms();
            _invoiceService.PaymentTermsMapper(payTerms, dto.Payment_terms);
            payTerms.InvoiceSell = dbInvoice;
            this._db.Entry(payTerms).State = EntityState.Added;
            
           
            foreach (var rate in dto.Rates_values_list)
            {

                var dbPos = this._invoiceService.NewInvoiceRateValueBasedOnDTOMapper(rate);
                dbPos.InvoiceSell = dbInvoice;
                this._db.Entry(dbPos).State = EntityState.Added;
            }

            dbInvoice.Seller = this._companyService.GetCompanyById(dto.Seller.CompanyId.Value);
            dbInvoice.SellingDate = dto.Selling_date;
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
            dbInvoice.DateOfIssue = invoiceDTO.Date_of_issue;
            dbInvoice.Info = invoiceDTO.Info;
            dbInvoice.SellingDate = invoiceDTO.Selling_date;
        }

        private InvoiceSellDTO EtoDTOInvoiceSell(InvoiceSell inv)
        {
            var res = new InvoiceSellDTO();
            res.CreationInfo = new bp.Pomocne.CommonFunctions().CreationInfoMapper((CreationInfo)inv);
            res.Buyer = _companyService.EtDTOCompany(inv.Buyer);
            res.Currency = this._invoiceService.EtDTOCurrency(inv.Currency);
            res.Date_of_issue = inv.DateOfIssue;
            res.Extra_info = (InvoiceExtraInfoDTO)this._invoiceService.EtoDTOExtraInfo(inv.ExtraInfo);
            if (inv.Load != null) {
                res.Extra_info.LoadId = inv.LoadId;
                res.Extra_info.LoadNo = inv.Load.LoadNo;
            }
            res.Info = inv.Info;
            res.Invoice_no = inv.InvoiceNo;
            foreach (var pos in inv.InvoicePosList)
            {
                res.Invoice_pos_list.Add(this._invoiceService.EtDTOInvoicePos(pos));
            }
            res.Invoice_sell_id = inv.InvoiceSellId;
            res.Invoice_total = _invoiceService.EtoDTOInvoiceTotal(inv.InvoiceTotal);

            res.Payment_terms = _invoiceService.EtDTOPaymentTerms(inv.PaymentTerms);
            foreach (var rate in inv.RatesValuesList)
            {
                res.Rates_values_list.Add(this._invoiceService.EtoDTORateValue(rate));
            }
            res.Seller = _companyService.EtDTOCompany(inv.Seller);
            res.Selling_date = inv.SellingDate;
            return res;
        }
        




        #endregion
    }
}
