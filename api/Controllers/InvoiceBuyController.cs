﻿using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Entities.Dane.Invoice;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using bp.ot.s.API.Entities.Dane.Company;
using System.Data.Common;
using System.Security.Claims;
using bp.shared.DTO;
using bp.shared;


namespace bp.ot.s.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Finanse")]
    public class InvoiceBuyController : Controller
    {
        private readonly BpKpirContextDane _db;
        private CompanyService _companyService;
        private readonly InvoiceService _invoiceService;
        private readonly CommonFunctions _commonFunctions;

        public InvoiceBuyController(BpKpirContextDane db, CompanyService companyService, InvoiceService invoiceService, CommonFunctions commonFunctions)
        {
            this._db = db;
            this._companyService = companyService;
            this._invoiceService = invoiceService;
            this._commonFunctions = commonFunctions;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var db = await this._invoiceService.QueryableInvoiceBuy()
                .FirstOrDefaultAsync(f=>f.InvoiceBuyId==id);

            if (db == null) {
                return NotFound();
            }

            await this._invoiceService.InvoiceBuyDelete(id, db);
            await this._db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{dateStart}/{dateEnd}")]
        public async Task<IActionResult> GetAll(DateTime dateStart, DateTime dateEnd)
        {
            var dateRange = new DateRangeDTO {
                DateEnd = dateEnd,
                DateStart = dateStart
            };

            var res = await this._invoiceService.InvoiceBuyGetAllToList(dateRange);
            return Ok(res);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res =  await this._invoiceService.InvoiceBuyGetById(id);
            if (res == null) {
                return BadRequest(bp.sharedLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Błąd", $"Nie znaleziono faktury zakupu o ID: {id}"));
            }
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentRemindList()
        {
            var dbRes = await this._invoiceService.QueryableInvoiceBuy()
                .Where(w => w.PaymentIsDone == false)
                .Select(s => s)
                .ToListAsync();

            var unpaid = new List<InvoicePaymentRemindDTO>();
            var notConfirmed = new List<InvoicePaymentRemindDTO>();

            foreach (var inv in dbRes)
            {
                var dto = new InvoicePaymentRemindDTO();
                this.EtoDTOBasicInvoicePaymentRemind(inv, dto);

                // paymentDate based on paymentterms..
                if (inv.PaymentTerms.PaymentDays.HasValue)
                {
                    dto.PaymentDate = inv.SellingDate.AddDays(inv.PaymentTerms.PaymentDays.Value);
                }
                else {
                    dto.PaymentDate = inv.SellingDate;
                }


                //check if invoice is recived (or it its only generated by created laod-buy..)
                if (inv.InvoiceReceived)
                {
                    //dto.PaymentDate = 
                    unpaid.Add(dto);
                }
                else {
                    notConfirmed.Add(dto);
                }
            }

            var res = new
            {
                Unpaid = unpaid.OrderBy(o => o.PaymentDate).ToList(),
                NotConfirmed = notConfirmed.OrderBy(o => o.PaymentDate).ToList()
            };


            return Ok(res);
        }

        [HttpPost]
        public IActionResult PostCalcRates([FromBody] InvoiceBuyDTO dto)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] InvoiceBuyDTO dto)
        {
            var dbInvoice = new InvoiceBuy();

            if (id > 0)
            {
                dbInvoice = await this._invoiceService.QueryableInvoiceBuy()
                        .FirstOrDefaultAsync(f => f.InvoiceBuyId == id);

                if (dbInvoice == null)
                {
                    return BadRequest(bp.sharedLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Błąd", $"Nie znaleziono faktury o ID: {id}"));
                }
            }
            else {
                //new entity
                this._db.Entry(dbInvoice).State = EntityState.Added;
            }

            await this._invoiceService.MapperInvoiceBuy(dbInvoice, dto, User);

            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (DbException)
            {

                throw;
            }
            return Ok(this._invoiceService.EtoDTOInvoiceBuy(dbInvoice));
        }

        private InvoicePaymentRemindDTO EtoDTOBasicInvoicePaymentRemind(InvoiceBuy db, InvoicePaymentRemindDTO dto)
        {
            var res = dto ?? new InvoicePaymentRemindDTO();

            res.Company = this._companyService.CompanyCardMapper(db.CompanySeller);
            res.Currency = this._invoiceService.EtoDTOCurrency(db.Currency);
            res.InvoiceId = db.InvoiceBuyId;
            res.InvoiceNo = db.InvoiceNo;
            //res.InvoiceTotal = this._invoiceService.EtDTOInvoiceTotal(db.InvoiceTotal);
            return res;
        }
               
    }
}
