using Microsoft.AspNetCore.Authentication.JwtBearer;
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

namespace bp.ot.s.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Finanse")]
    public class InvoiceSellController:Controller
    {
        private readonly OfferTransDbContextDane _db;
        private readonly PdfRaports _pdf;
        private readonly CompanyService _companyService;
        private InvoiceService _invoiceService;

        public InvoiceSellController(OfferTransDbContextDane db, PdfRaports pdf, CompanyService companyService, InvoiceService invoiceService)
        {
            this._db = db;
            this._pdf = pdf;
            this._companyService = companyService;
            this._invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var dbRes = await this._db.InvoiceSell
                .Include(i => i.Buyer.AddressList)
                .Include(i => i.Buyer.EmployeeList)
                .Include(i => i.Buyer.BankAccountList)
                .Include(i => i.Currency)
                .Include(i => i.InvoicePosList)
                .Include(i => i.PaymentTerm)
                .Include(i => i.RatesValuesList)
                .Include(i => i.Seller.AddressList)
                .Include(i => i.Seller.EmployeeList)
                .Include(i => i.Seller.BankAccountList)
                .OrderByDescending(o=>o.InvoiceSellId)
                .ToListAsync();

            List<InvoiceSellDTO> resList = new List<InvoiceSellDTO>();
            foreach (var invoice in dbRes)
            {
                resList.Add(this.EtoDTOInvoiceSell(invoice));
            }

            return Ok(resList);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dbRes= await this._db.InvoiceSell
                .Include(i => i.Buyer.AddressList)
                .Include(i => i.Buyer.EmployeeList)
                .Include(i => i.Buyer.BankAccountList)
                .Include(i => i.Currency)
                .Include(i => i.InvoicePosList)
                .Include(i => i.PaymentTerm)
                .Include(i => i.RatesValuesList)
                .Include(i => i.Seller.AddressList)
                .Include(i => i.Seller.EmployeeList)
                .Include(i => i.Seller.BankAccountList)
                .Where(w => w.InvoiceSellId == id).FirstOrDefaultAsync();

            return Ok(this.EtoDTOInvoiceSell(dbRes));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] InvoiceSellDTO invoiceDTO)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbInvoice = await this._db.InvoiceSell
                .Include(i => i.Buyer.AddressList)
                .Include(i => i.Buyer.EmployeeList)
                .Include(i => i.Buyer.BankAccountList)
                .Include(i => i.Currency)
                .Include(i => i.InvoicePosList)
                .Include(i => i.PaymentTerm)
                .Include(i => i.Seller.AddressList)
                .Include(i => i.Seller.EmployeeList)
                .Include(i => i.Seller.BankAccountList)
                .Where(w => w.InvoiceSellId == id).FirstOrDefaultAsync();

            if (dbInvoice == null) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono faktury o Id: {id}"));
            }

            // diff buyerID
            if (dbInvoice.Buyer.CompanyId != invoiceDTO.Buyer.CompanyId) {
                dbInvoice.Buyer = this._db.Comapny.Where(w => w.CompanyId == invoiceDTO.Buyer.CompanyId).FirstOrDefault();
            }
            if (dbInvoice.Currency.CurrencyId != invoiceDTO.Currency.CurrencyId) {
                dbInvoice.Currency = _db.Currency.Where(w => w.CurrencyId == invoiceDTO.Currency.CurrencyId).FirstOrDefault();
            }
            this.BasicDTOtoEntityMapping(dbInvoice, invoiceDTO);

            //remove deleted pos
            foreach (var pos in dbInvoice.InvoicePosList)
            {
                if (!invoiceDTO.Invoice_pos_list.Any(a => a.Invoice_pos_id == pos.InvoicePosId)) {
                    this._db.Entry(pos).State = EntityState.Deleted;
                }
            }

            //modify or add pos
            foreach (var pos in invoiceDTO.Invoice_pos_list)
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
                    this._db.Entry(posDb).State = EntityState.Modified;
                }
            }


            //remove rate value
            foreach (var rate in dbInvoice.RatesValuesList)
            {
                if(!invoiceDTO.Rates_values_list.Any(a => a.Invoice_rates_values_id == rate.RateValueId)) {
                    this._db.Entry(rate).State = EntityState.Deleted;
                }
            }

            //modify or add rateValue
            foreach (var rate in invoiceDTO.Rates_values_list)
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
                    this._db.Entry(dbRate).State = EntityState.Modified;
                }
            }
            //paymentTerms...
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


            await this._db.SaveChangesAsync();

            return Ok(this.EtoDTOInvoiceSell(dbInvoice));
        }


        [HttpPost]
        public async  Task<IActionResult> Post([FromBody] InvoiceSellDTO invoiceDTO)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbInvoice = new InvoiceSell();
            dbInvoice.Buyer = this._companyService.GetCompanyById(invoiceDTO.Buyer.CompanyId);
            dbInvoice.Currency = this._db.Currency.Where(w => w.CurrencyId == invoiceDTO.Currency.CurrencyId).FirstOrDefault();
            dbInvoice.InvoiceNo = new DocNumber().GenNumberMonthYearNumber(this._db.InvoiceSell.LastOrDefault()?.InvoiceNo, invoiceDTO.Selling_date).DocNumberCombined;
            dbInvoice.Seller = this._companyService.GetCompanyById(invoiceDTO.Seller.CompanyId);
            this.BasicDTOtoEntityMapping(dbInvoice, invoiceDTO);
            this._db.Entry(dbInvoice).State = Microsoft.EntityFrameworkCore.EntityState.Added;

            //paymentTerms...
            if (invoiceDTO.Payment_terms.PaymentTerm.IsDescription)
            {
                dbInvoice.PaymentDescription = invoiceDTO.Payment_terms.Description;
            }
            else {
                dbInvoice.PaymentDescription = null;
            }
            if (invoiceDTO.Payment_terms.PaymentTerm.IsPaymentDate)
            {
                dbInvoice.PaymentDate = invoiceDTO.Payment_terms.PaymentDate;
                dbInvoice.PaymentDays = invoiceDTO.Payment_terms.PaymentDays;
            }
            else {
                dbInvoice.PaymentDate = null;
                dbInvoice.PaymentDays = null;
            }
            if (dbInvoice.PaymentTerm == null || (dbInvoice.PaymentTerm!=null && dbInvoice.PaymentTerm.PaymentTermId != invoiceDTO.Payment_terms.PaymentTerm.PaymentTermId)) {
                dbInvoice.PaymentTerm = this._db.PaymentTerm.Find(invoiceDTO.Payment_terms.PaymentTerm.PaymentTermId);
            }




            foreach (var pos in invoiceDTO.Invoice_pos_list)
            {
                var dbPos = this._invoiceService.NewInvoicePosBasedOnDTOMapper(pos);
                dbPos.InvoiceSell = dbInvoice;
                this._db.Entry(dbPos).State= EntityState.Added;
            }

            foreach (var rate in invoiceDTO.Rates_values_list)
            {

                var dbPos = this._invoiceService.NewInvoiceRateValueBasedOnDTOMapper(rate);
                dbPos.InvoiceSell = dbInvoice;
                this._db.Entry(dbPos).State = EntityState.Added;
            }

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dbRes = await this._db.InvoiceSell
                .Include(i => i.Buyer.AddressList)
                .Include(i => i.Buyer.EmployeeList)
                .Include(i => i.Buyer.BankAccountList)
                .Include(i => i.Currency)
                .Include(i => i.InvoicePosList)
                .Include(i => i.PaymentTerm)
                .Include(i => i.RatesValuesList)
                .Include(i => i.Seller.AddressList)
                .Include(i => i.Seller.EmployeeList)
                .Include(i => i.Seller.BankAccountList)
                .Where(w => w.InvoiceSellId == id).FirstOrDefaultAsync();

            if (dbRes == null) { return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Delete", $"Nie znaleziono faktury o ID: {id} ")); }


            foreach (var rate in dbRes.RatesValuesList)
            {
                this._db.Entry(rate).State = EntityState.Deleted;
            }

            foreach (var pos in dbRes.InvoicePosList)
            {
                this._db.Entry(pos).State = EntityState.Deleted;
            }

            this._db.Entry(dbRes).State = EntityState.Deleted;

            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception e)
            {

                throw;
            }

            
            

            return Ok("Usunięto, wszystko ok");
        }


        
        
        #region private helpers

        private void BasicDTOtoEntityMapping(InvoiceSell dbInvoice, InvoiceSellDTO invoiceDTO)
        {
            dbInvoice.DateOfIssue = invoiceDTO.Date_of_issue;
            // is laodNo
            if (invoiceDTO.Extra_info.Is_load_no)
            {
                dbInvoice.ExtraInfo_IsLoadNo = true;
                dbInvoice.ExtraInfo_LoadNo = invoiceDTO.Extra_info.Load_no;
            }
            else {
                dbInvoice.ExtraInfo_LoadNo = null;
                dbInvoice.ExtraInfo_IsLoadNo = false;
            }

            //nbp exchanged
            if (invoiceDTO.Extra_info.Is_tax_nbp_exchanged)
            {
                dbInvoice.ExtraInfo_IsTaxNbpExchanged = true;
                dbInvoice.ExtraInfo_TaxExchangedInfo = invoiceDTO.Extra_info.Tax_exchanged_info;
            }
            else {
                dbInvoice.ExtraInfo_IsTaxNbpExchanged = false;
                dbInvoice.ExtraInfo_TaxExchangedInfo = null;
            }

            dbInvoice.Info = invoiceDTO.Info;
            dbInvoice.SellingDate = invoiceDTO.Selling_date;
            dbInvoice.TotalBrutto = invoiceDTO.Invoice_total.Total_brutto;
            dbInvoice.TotalNetto = invoiceDTO.Invoice_total.Total_netto;
            dbInvoice.TotalTax = invoiceDTO.Invoice_total.Total_tax;
        }

        private InvoiceSellDTO EtoDTOInvoiceSell(InvoiceSell inv)
        {
            var res = new InvoiceSellDTO();
            res.Buyer = _companyService.EntityToDTOCompany(inv.Buyer);
            res.Currency = this._invoiceService.EtoDTOCurrency(inv.Currency);
            res.Date_of_issue = inv.DateOfIssue;
            res.Extra_info = this.EtoDTOExtraInfo(inv);
            res.Info = inv.Info;
            res.Invoice_no = inv.InvoiceNo;
            foreach (var pos in inv.InvoicePosList)
            {
                res.Invoice_pos_list.Add(this._invoiceService.EtoDTOInvoicePos(pos));
            }
            res.Invoice_sell_id = inv.InvoiceSellId;
            res.Invoice_total = this._invoiceService.EtoDTOInvoiceTotal(inv);
            res.Payment_terms = this._invoiceService.EtoDTOPaymentTermsInvoiceSell(inv);
            foreach (var rate in inv.RatesValuesList)
            {
                res.Rates_values_list.Add(this._invoiceService.EtoDTORateValue(rate));
            }
            res.Seller = _companyService.EntityToDTOCompany(inv.Seller);
            res.Selling_date = inv.SellingDate;

            return res;
        }
        
        private InvoiceExtraInfoDTO EtoDTOExtraInfo(InvoiceSell inv)
        {
            var res = new InvoiceExtraInfoDTO();
            res.Is_load_no = inv.ExtraInfo_IsLoadNo;
            res.Is_tax_nbp_exchanged = inv.ExtraInfo_IsTaxNbpExchanged;
            res.Load_no = inv.ExtraInfo_LoadNo;
            res.Tax_exchanged_info = inv.ExtraInfo_TaxExchangedInfo;

            return res;
        }

        #endregion
    }
}
