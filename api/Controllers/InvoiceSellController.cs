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


        public InvoiceSellController(OfferTransDbContextDane db, PdfRaports pdf, CompanyService companyService)
        {
            this._db = db;
            this._pdf = pdf;
            this._companyService = companyService;
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
                .Include(i => i.PaymentTerms.PaymentTerm)
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
                .Include(i=>i.Currency)
                .Include(i => i.InvoicePosList)
                .Include(i => i.PaymentTerms.PaymentTerm)
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
                .Include(i => i.PaymentTerms.PaymentTerm)
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
                    var pDb = this.NewInvoicePosBasedOnDTOMapper(pos);
                    pDb.InvoiceSell = dbInvoice;
                    this._db.Entry(pDb).State = EntityState.Added;
                }
                else {
                    posDb.BruttoValue = pos.Brutto_value;
                    posDb.MeasurementUnit = pos.Measurement_unit;
                    posDb.Name = pos.Name;
                    posDb.NettoValue = pos.Netto_value;
                    posDb.Pkwiu = pos.Pkwiu;
                    posDb.Quantity = pos.Quantity;
                    posDb.UnitPrice = pos.Unit_price;
                    posDb.VatRate = pos.Vat_rate;
                    posDb.VatUnitValue = pos.Vat_unit_value;
                    posDb.VatValue = pos.Vat_value;

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
                    var newRate = this.NewInvoiceRateValueBasedOnDTOMapper(rate);
                    newRate.InvoiceSell = dbInvoice;
                    this._db.Entry(newRate).State = EntityState.Added;
                }
                else {
                    dbRate.BruttoValue = rate.Brutto_value;
                    dbRate.NettoValue = rate.Netto_value;
                    dbRate.VatRate = rate.Vat_rate;
                    dbRate.VatValue = rate.Vat_value;

                    this._db.Entry(dbRate).State = EntityState.Modified;
                }
            }
            dbInvoice.PaymentTerms = this.PaymentTermsBasedOnDTOMapper(invoiceDTO.Payment_terms, dbInvoice.PaymentTerms);
            this._db.Entry(dbInvoice.PaymentTerms).State = EntityState.Modified;


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
            dbInvoice.Buyer = this._db.Comapny.Where(w => w.CompanyId == invoiceDTO.Buyer.CompanyId).FirstOrDefault();
            dbInvoice.Currency = this._db.Currency.Where(w => w.CurrencyId == invoiceDTO.Currency.CurrencyId).FirstOrDefault();
            dbInvoice.InvoiceNo = new DocNumber().GenNumberMonthYearNumber(this._db.InvoiceSell.LastOrDefault().InvoiceNo, invoiceDTO.Selling_date).DocNumberCombined;
            dbInvoice.Seller = this._db.Comapny.Where(w => w.CompanyId == invoiceDTO.Seller.CompanyId).FirstOrDefault();
            this.BasicDTOtoEntityMapping(dbInvoice, invoiceDTO);
            this._db.Entry(dbInvoice).State = Microsoft.EntityFrameworkCore.EntityState.Added;

            var dbTerms = new PaymentTerms();
            dbTerms = this.PaymentTermsBasedOnDTOMapper(invoiceDTO.Payment_terms, dbTerms);
            dbTerms.InvoiceSell = dbInvoice;
            this._db.Entry(dbTerms).State = EntityState.Added;

            foreach (var pos in invoiceDTO.Invoice_pos_list)
            {
                var dbPos = this.NewInvoicePosBasedOnDTOMapper(pos);
                dbPos.InvoiceSell = dbInvoice;
                this._db.Entry(dbPos).State= EntityState.Added;
            }

            foreach (var rate in invoiceDTO.Rates_values_list)
            {

                var dbPos = this.NewInvoiceRateValueBasedOnDTOMapper(rate);
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

        private InvoicePos NewInvoicePosBasedOnDTOMapper(InvoicePosDTO posDTO) {
            var pos = new InvoicePos();

            pos.BruttoValue = posDTO.Brutto_value;
            pos.MeasurementUnit = posDTO.Measurement_unit;
            pos.Name = posDTO.Name;
            pos.NettoValue = posDTO.Netto_value;
            pos.Pkwiu = posDTO.Pkwiu;
            pos.Quantity = posDTO.Quantity;
            pos.UnitPrice = posDTO.Unit_price;
            pos.VatRate = posDTO.Vat_rate;
            pos.VatUnitValue = posDTO.Vat_unit_value;
            pos.VatValue = posDTO.Vat_value;
            return pos;
        }

        private RateValue NewInvoiceRateValueBasedOnDTOMapper(InvoiceRatesValuesDTO rateDTO)
        {
            var rate = new RateValue();

            rate.BruttoValue = rateDTO.Brutto_value;
            rate.NettoValue = rateDTO.Netto_value;
            rate.VatRate = rateDTO.Vat_rate;
            rate.VatValue = rateDTO.Vat_value;

            return rate;
        }

        private PaymentTerms PaymentTermsBasedOnDTOMapper(PaymentTermsDTO pDTO, PaymentTerms dbTerms)
        {
            dbTerms.Day0 = pDTO.Day0;
            dbTerms.Description = pDTO.PaymentTerm.IsDescription ? pDTO.Description : null;

            if (pDTO.PaymentTerm.IsPaymentDate) {dbTerms.PaymentDate = pDTO.PaymentDate.Value;}
            if (pDTO.PaymentTerm.IsPaymentDate) { dbTerms.PaymentDays = pDTO.PaymentDays.Value;}
            dbTerms.PaymentTerm = this._db.PaymentTerm.Where(w => w.PaymentTermId == pDTO.PaymentTerm.PaymentTermId).FirstOrDefault();
            return dbTerms;
        }

        private InvoiceSellDTO EtoDTOInvoiceSell(InvoiceSell inv)
        {
            var res = new InvoiceSellDTO();
            res.Buyer = _companyService.EntityToDTOCompany(inv.Buyer);
            res.Currency = this.EtoDTOCurrency(inv.Currency);
            res.Date_of_issue = inv.DateOfIssue;
            res.Extra_info = this.EtoDTOExtraInfo(inv);
            res.Info = inv.Info;
            res.Invoice_no = inv.InvoiceNo;
            foreach (var pos in inv.InvoicePosList)
            {
                res.Invoice_pos_list.Add(this.EtoDTOInvoicePos(pos));
            }
            res.Invoice_sell_id = inv.InvoiceSellId;
            res.Invoice_total = this.EtoDTOInvoiceTotal(inv);
            res.Payment_terms = this.EtoDTOPaymentTerms(inv.PaymentTerms);
            foreach (var rate in inv.RatesValuesList)
            {
                res.Rates_values_list.Add(this.EtoDTORateValue(rate));
            }
            res.Seller = _companyService.EntityToDTOCompany(inv.Seller);
            res.Selling_date = inv.SellingDate;

            return res;
        }
        
        private CurrencyDTO EtoDTOCurrency(Currency curr) {
            var res = new CurrencyDTO();
            res.CurrencyId = curr.CurrencyId;
            res.Description = curr.Description;
            res.Name = curr.Name;

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

        private InvoicePosDTO EtoDTOInvoicePos(InvoicePos pos) {
            var res = new InvoicePosDTO();
            res.Brutto_value = pos.BruttoValue;
            res.Invoice_pos_id = pos.InvoicePosId;
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

        private InvoiceTotalDTO EtoDTOInvoiceTotal(InvoiceSell inv) {
            var res = new InvoiceTotalDTO();
            res.Total_brutto = inv.TotalBrutto;
            res.Total_netto = inv.TotalNetto;
            res.Total_tax = inv.TotalTax;
            return res;
        }

        private PaymentTermsDTO EtoDTOPaymentTerms(PaymentTerms terms)
        {
            var res = new PaymentTermsDTO();
            res.Day0 = terms.Day0;
            res.Description = terms.Description;
            if (terms.PaymentDate.HasValue) {
                res.PaymentDate = terms.PaymentDate.Value;
            }
            if (terms.PaymentDays.HasValue) {
                res.PaymentDays = terms.PaymentDays.Value;
            }
            res.PaymentTermsId = terms.PaymentTermsId;
            res.PaymentTerm = new PaymentTermDTO
            {
                IsDescription=terms.PaymentTerm.IsDescription,
                IsPaymentDate= terms.PaymentTerm.IsPaymentDate,
                Name= terms.PaymentTerm.Name,
                PaymentTermId= terms.PaymentTerm.PaymentTermId
            };
            return res;
        }

        private InvoiceRatesValuesDTO EtoDTORateValue(RateValue rate) {
            var res = new InvoiceRatesValuesDTO();

            res.Brutto_value = rate.BruttoValue;
            res.Invoice_rates_values_id = rate.RateValueId;
            res.Netto_value = rate.NettoValue;
            res.Vat_rate = rate.VatRate;
            res.Vat_value = rate.VatValue;

            return res;
        }

        #endregion
    }
}
