using bp.ot.s.API.Entities.Context;
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

namespace bp.ot.s.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Finanse")]
    public class InvoiceBuyController: Controller
    {
        private readonly OfferTransDbContextDane _db;
        private CompanyService _companyService;
        private readonly InvoiceService _invoiceService;

        public InvoiceBuyController(OfferTransDbContextDane db, CompanyService companyService, InvoiceService invoiceService)
        {
            this._db = db;
            this._companyService = companyService;
            this._invoiceService = invoiceService;
        }

        public async Task<IActionResult> GetAll()
        {
            var dbResList = await this._invoiceService.InvoiceBuyQueryable()
                    .OrderByDescending(o => o.InvoiceBuyId)
                    .ToListAsync();


            var res = new List<InvoiceBuyDTO>();

            foreach (var inv in dbResList)
            {
                res.Add(this.EtoDTOInvoiceBuy(inv));
            }


            return Ok(res);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dbInvoice = await this._invoiceService.InvoiceBuyQueryable()
                .Where(w => w.InvoiceBuyId == id)
                .FirstOrDefaultAsync();

            if (dbInvoice == null) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Błąd", $"Nie znaleziono faktury zakupu o ID: {id}"));
            }


            return Ok(this.EtoDTOInvoiceBuy(dbInvoice));
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] InvoiceBuyDTO invoiceDTO)
        {
            var dbInvoice = await this._invoiceService.InvoiceBuyQueryable()
                    .FirstOrDefaultAsync(f => f.InvoiceBuyId == id);

            if (dbInvoice == null) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Błąd", $"Nie znaleziono faktury o ID: {id}"));
            }

            if (dbInvoice.Currency.CurrencyId != invoiceDTO.Currency.CurrencyId)
            {
                dbInvoice.Currency = _invoiceService._currencyList.Where(w => w.CurrencyId == invoiceDTO.Currency.CurrencyId).FirstOrDefault();
                this._db.Entry(dbInvoice.Currency).State = EntityState.Modified;
            }

            dbInvoice.DateOfIssue = invoiceDTO.Date_of_issue;
            dbInvoice.Info = invoiceDTO.Info;
            dbInvoice.InvoiceNo = invoiceDTO.Invoice_no;

            //remove deleted pos
            foreach (var dbPos in dbInvoice.InvoicePosList)
            {
                var found = invoiceDTO.Invoice_pos_list.Where(w => w.Invoice_pos_id == dbPos.InvoicePosId).FirstOrDefault();
                if (found == null) {
                    this._db.Entry(dbPos).State = EntityState.Deleted;
                }
            }
            //modify or Add pos
            foreach (var pos in invoiceDTO.Invoice_pos_list)
            {
                var dbPos = dbInvoice.InvoicePosList.Where(w => w.InvoicePosId== pos.Invoice_pos_id).FirstOrDefault();
                if (dbPos != null)
                {
                    this._invoiceService.InvoicePosMapperFromDTO(dbPos, pos);
                    //this._db.Entry(dbPos).State = EntityState.Modified;
                }
                else {
                    //new entity
                    var newPos=this._invoiceService.NewInvoicePosBasedOnDTOMapper(pos);
                    this._db.Entry(newPos).State = EntityState.Added;
                }
            }

            this._invoiceService.InvoiceTotalMapper(dbInvoice.InvoiceTotal, invoiceDTO.Invoice_total);
            this._invoiceService.PaymentTermsMapper(dbInvoice.PaymentTerms, invoiceDTO.Payment_terms);

            //remove deleted taxValues
            foreach (var tax in dbInvoice.RatesValuesList)
            {
                var dbTax = invoiceDTO.Rates_values_list.Where(w => w.Invoice_rates_values_id == tax.RateValueId).FirstOrDefault();
                if (dbTax == null) {
                    this._db.Entry(tax).State = EntityState.Deleted;
                }
            }

            //modify or add new tax
            foreach (var rateDTO in invoiceDTO.Rates_values_list)
            {
                var dbTax = dbInvoice.RatesValuesList.Where(w => w.RateValueId == rateDTO.Invoice_rates_values_id).FirstOrDefault();
                if (dbTax != null)
                {
                    this._invoiceService.InvoiceTaxValueMapperFromDTO(dbTax, rateDTO);
                    this._db.Entry(dbTax).State = EntityState.Modified;
                }
                else {
                    var newRate = this._invoiceService.NewInvoiceRateValueBasedOnDTOMapper(rateDTO);
                    this._db.Entry(newRate).State = EntityState.Added;
                }
            }

            
           
            if (dbInvoice.Seller.CompanyId != invoiceDTO.Seller.CompanyId) {
                dbInvoice.Seller = this._db.Company.Find(invoiceDTO.Seller.CompanyId);
            }
            dbInvoice.SellingDate = invoiceDTO.Selling_date;
            //dbInvoice.TotalBrutto = invoiceDTO.Invoice_total.Total_brutto;
            //dbInvoice.TotalNetto = invoiceDTO.Invoice_total.Total_netto;
            //dbInvoice.TotalTax = invoiceDTO.Invoice_total.Total_tax;

            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (DbException)
            {

                throw;
            }
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] InvoiceBuyDTO invoiceDTO)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            var dbInvoice = new InvoiceBuy();

            dbInvoice.Currency = this._invoiceService._currencyList.Where(w => w.CurrencyId == invoiceDTO.Currency.CurrencyId).FirstOrDefault();
            dbInvoice.DateOfIssue = invoiceDTO.Date_of_issue;
            dbInvoice.Info = invoiceDTO.Info;
            dbInvoice.InvoiceNo = invoiceDTO.Invoice_no;

            foreach (var pos in invoiceDTO.Invoice_pos_list)
            {
                var dbPos = this._invoiceService.NewInvoicePosBasedOnDTOMapper(pos);
                dbPos.InvoiceBuy = dbInvoice;
                this._db.Entry(dbPos).State = EntityState.Added;
            }


            var invTotal = new InvoiceTotal();
            _invoiceService.InvoiceTotalMapper(invTotal, invoiceDTO.Invoice_total);
            invTotal.InvoiceBuy = dbInvoice;
            this._db.Entry(invTotal).State = EntityState.Added;


            var paymentTerms = new PaymentTerms();
            _invoiceService.PaymentTermsMapper(paymentTerms, invoiceDTO.Payment_terms);
            paymentTerms.InvoiceBuy = dbInvoice;
            this._db.Entry(paymentTerms).State = EntityState.Added;

            foreach (var rate in invoiceDTO.Rates_values_list)
            {
                var dbRate = this._invoiceService.NewInvoiceRateValueBasedOnDTOMapper(rate);
                dbRate.InvoiceBuy = dbInvoice;
                
                this._db.Entry(dbRate).State = EntityState.Added;
            }

            dbInvoice.Seller = this._db.Company.Where(w => w.CompanyId == invoiceDTO.Seller.CompanyId).FirstOrDefault();
            dbInvoice.SellingDate = invoiceDTO.Selling_date;

            this._db.Entry(dbInvoice).State = EntityState.Added;

            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (DbException)
            {

                throw;
            }


            return Ok();
        }

        private InvoiceBuyDTO EtoDTOInvoiceBuy(InvoiceBuy inv)
        {
            var res = new InvoiceBuyDTO();
            res.Currency = this._invoiceService.EtDTOCurrency(inv.Currency);
            res.Date_of_issue = inv.DateOfIssue;
            res.Info = inv.Info;
            res.Invoice_buy_id = inv.InvoiceBuyId;
            res.Invoice_no = inv.InvoiceNo;
            foreach (var pos in inv.InvoicePosList)
            {
                res.Invoice_pos_list.Add(this._invoiceService.EtDTOInvoicePos(pos));
            }
            res.Invoice_total = _invoiceService.EtoDTOInvoiceTotal(inv.InvoiceTotal);
            res.Payment_terms = this._invoiceService.EtDTOPaymentTerms(inv.PaymentTerms);
            foreach (var rate in inv.RatesValuesList)
            {
                res.Rates_values_list.Add(this._invoiceService.EtoDTORateValue(rate));
            }
            res.Seller = _companyService.EtDTOCompany(inv.Seller);
            res.Selling_date = inv.SellingDate;

            return res;
        }
    }
}
