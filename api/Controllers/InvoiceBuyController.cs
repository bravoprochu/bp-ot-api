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
            var dbResList = await this._db.InvoiceBuy
                    .Include(i => i.Currency)
                    .Include(i => i.InvoicePosList)
                    .Include(i => i.PaymentTerm)
                    .Include(i => i.RatesValuesList)
                    .Include(i => i.Seller.AddressList)
                    .Include(i => i.Seller.EmployeeList)
                    .Include(i => i.Seller.BankAccountList)
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
            var dbInvoice = await this._db.InvoiceBuy
                .Include(i => i.Currency)
                .Include(i => i.InvoicePosList)
                .Include(i => i.PaymentTerm)
                .Include(i => i.RatesValuesList)
                .Include(i => i.Seller.AddressList)
                .Include(i => i.Seller.EmployeeList)
                .Include(i => i.Seller.BankAccountList)
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
            var dbInvoice = await this._db.InvoiceBuy
                    .Include(i => i.Currency)
                    .Include(i => i.InvoicePosList)
                    .Include(i => i.PaymentTerm)
                    .Include(i => i.RatesValuesList)
                    .Include(i => i.Seller.AddressList)
                    .Include(i => i.Seller.EmployeeList)
                    .Include(i => i.Seller.BankAccountList)
                    .FirstOrDefaultAsync();

            if (dbInvoice == null) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Błąd", $"Nie znaleziono faktury o ID: {id}"));
            }

            if (dbInvoice.Currency.CurrencyId != invoiceDTO.Currency.CurrencyId)
            {
                dbInvoice.Currency = await this._db.Currency.Where(w => w.CurrencyId == invoiceDTO.Currency.CurrencyId).FirstOrDefaultAsync();
                this._db.Entry(dbInvoice.Currency).State = EntityState.Modified;
            }

            dbInvoice.DateOfIssue = invoiceDTO.Date_of_issue;
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
                    this._db.Entry(dbPos).State = EntityState.Modified;
                }
                else {
                    //new entity
                    var newPos=this._invoiceService.NewInvoicePosBasedOnDTOMapper(pos);
                    this._db.Entry(newPos).State = EntityState.Added;
                }
            }


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

            //paymentTerms

            if (invoiceDTO.Payment_terms.PaymentTerm.IsDescription)
            {
                dbInvoice.PaymentDescription = invoiceDTO.Payment_terms.Description;
            }
            else
            {
                dbInvoice.PaymentDescription = null;
            }
            if (invoiceDTO.Payment_terms.PaymentTerm.IsPaymentDate)
            {
                dbInvoice.PaymentDate = invoiceDTO.Payment_terms.PaymentDate;
                dbInvoice.PaymentDays = invoiceDTO.Payment_terms.PaymentDays;
            }
            else
            {
                dbInvoice.PaymentDate = null;
                dbInvoice.PaymentDays = null;
            }
            if (dbInvoice.PaymentTerm == null || (dbInvoice.PaymentTerm != null && dbInvoice.PaymentTerm.PaymentTermId != invoiceDTO.Payment_terms.PaymentTerm.PaymentTermId))
            {
                dbInvoice.PaymentTerm = this._db.PaymentTerm.Find(invoiceDTO.Payment_terms.PaymentTerm.PaymentTermId);
            }




            if (dbInvoice.Seller.CompanyId != invoiceDTO.Seller.CompanyId) {
                dbInvoice.Seller = this._db.Comapny.Find(invoiceDTO.Seller.CompanyId);
            }
            dbInvoice.SellingDate = invoiceDTO.Selling_date;
            dbInvoice.TotalBrutto = invoiceDTO.Invoice_total.Total_brutto;
            dbInvoice.TotalNetto = invoiceDTO.Invoice_total.Total_netto;
            dbInvoice.TotalTax = invoiceDTO.Invoice_total.Total_tax;

            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception e)
            {

                throw;
            }
            return Ok(this.EtoDTOInvoiceBuy(dbInvoice));
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] InvoiceBuyDTO invoiceDTO)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbInvoice = new InvoiceBuy();
            dbInvoice.Currency = this._db.Currency.Where(w => w.CurrencyId == invoiceDTO.Currency.CurrencyId).FirstOrDefault();
            dbInvoice.DateOfIssue = invoiceDTO.Date_of_issue;
            dbInvoice.InvoiceNo = invoiceDTO.Invoice_no;
            dbInvoice.Seller = this._db.Comapny.Where(w => w.CompanyId == invoiceDTO.Seller.CompanyId).FirstOrDefault();

            foreach (var pos in invoiceDTO.Invoice_pos_list)
            {
                var dbPos = this._invoiceService.NewInvoicePosBasedOnDTOMapper(pos);
                dbPos.InvoiceBuy = dbInvoice;
                this._db.Entry(dbPos).State = EntityState.Added;
            }

            foreach (var rate in invoiceDTO.Rates_values_list)
            {
                var dbRate = this._invoiceService.NewInvoiceRateValueBasedOnDTOMapper(rate);
                dbRate.InvoiceBuy = dbInvoice;
                
                this._db.Entry(dbRate).State = EntityState.Added;
            }

            //paymentTerms
            if (invoiceDTO.Payment_terms.PaymentTerm.IsDescription)
            {
                dbInvoice.PaymentDescription = invoiceDTO.Payment_terms.Description;
            }
            else
            {
                dbInvoice.PaymentDescription = null;
            }
            if (invoiceDTO.Payment_terms.PaymentTerm.IsPaymentDate)
            {
                dbInvoice.PaymentDate = invoiceDTO.Payment_terms.PaymentDate;
                dbInvoice.PaymentDays = invoiceDTO.Payment_terms.PaymentDays;
            }
            else
            {
                dbInvoice.PaymentDate = null;
                dbInvoice.PaymentDays = null;
            }
            if (dbInvoice.PaymentTerm == null || (dbInvoice.PaymentTerm != null && dbInvoice.PaymentTerm.PaymentTermId != invoiceDTO.Payment_terms.PaymentTerm.PaymentTermId))
            {
                dbInvoice.PaymentTerm = this._db.PaymentTerm.Find(invoiceDTO.Payment_terms.PaymentTerm.PaymentTermId);
            }


            dbInvoice.SellingDate = invoiceDTO.Selling_date;
            dbInvoice.TotalBrutto = invoiceDTO.Invoice_total.Total_brutto;
            dbInvoice.TotalNetto = invoiceDTO.Invoice_total.Total_netto;
            dbInvoice.TotalTax = invoiceDTO.Invoice_total.Total_tax;

            this._db.Entry(dbInvoice).State = EntityState.Added;

            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception e)
            {

                throw;
            }



            return Ok();
        }



        private InvoiceBuyDTO EtoDTOInvoiceBuy(InvoiceBuy inv)
        {
            var res = new InvoiceBuyDTO();
            res.Currency = this._invoiceService.EtoDTOCurrency(inv.Currency);
            res.Date_of_issue = inv.DateOfIssue;
            res.Info = inv.Info;
            res.Invoice_no = inv.InvoiceNo;
            foreach (var pos in inv.InvoicePosList)
            {
                res.Invoice_pos_list.Add(this._invoiceService.EtoDTOInvoicePos(pos));
            }
            res.Invoice_buy_id = inv.InvoiceBuyId;
            res.Invoice_total = new InvoiceTotalDTO
            {
                Total_brutto = inv.TotalBrutto,
                Total_netto = inv.TotalNetto,
                Total_tax = inv.TotalTax
            };
            res.Payment_terms = this._invoiceService.EtoDTOPaymentTermsInvoiceBuy(inv);
            foreach (var rate in inv.RatesValuesList)
            {
                res.Rates_values_list.Add(this._invoiceService.EtoDTORateValue(rate));
            }
            res.Seller = _companyService.EntityToDTOCompany(inv.Seller);
            res.Selling_date = inv.SellingDate;

            return res;

        }
    }
}
