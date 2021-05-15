using bp.kpir.DAO.Invoice;
using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Services;
using bp.ot.s.API.PomocneLocal.Constants;
using bp.shared;
using bp.shared.DTO;
using bp.shared.Linq;
using bp.sharedLocal.Pdf;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using bp.ot.s.API.Models.InvoiceSellPaymentStatus;

namespace bp.ot.s.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Finanse")]
    public class InvoiceSellController : Controller
    {
        private readonly BpKpirContextDane _db;
        private readonly PdfRaports _pdf;
        private readonly CompanyService _companyService;
        private InvoiceService _invoiceService;
        private CommonFunctions _commonFunctions;

        public InvoiceSellController(BpKpirContextDane db, PdfRaports pdf, CompanyService companyService, InvoiceService invoiceService, CommonFunctions commonFunctions)
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
            await this._invoiceService.InvoiceSellDelete(id);


            await this._db.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("{dateStart}/{dateEnd}")]
        public async Task<IActionResult> GetAll(DateTime dateStart, DateTime dateEnd)
        {
            var dateRange = new DateRangeDTO
            {
                DateEnd = dateEnd,
                DateStart = dateStart
            };

            var dateRangeFixedHours = shared.DateHelp.DateHelpful.DateRangeFixedHours(dateRange);
            var res = await this._invoiceService.InvoiceSellGetAllToList(dateRangeFixedHours);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetAll([FromBody] DateRangeDTO dateRange)
        {
            var res = await this._invoiceService.InvoiceSellGetAllToList(dateRange);
            return Ok(res);
        }





        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var db = await this._invoiceService.InvoiceSellGetById(id);
            if (db == null) { return NotFound(); }

            if (db.IsCorrection)
            {
                var org = await this._invoiceService.InvoiceSellGetById(db.BaseInvoiceId);
                if (org == null) { return NotFound(); }
                return Ok(this._invoiceService.EtoDTOInvoiceSellForInvoiceCorrection(db, org));
            }
            return Ok(db);
        }

        [HttpGet("{monthsAgo}")]
        public async Task<IActionResult> GetLastMonthInvoices(int monthsAgo)
        {
            var res = await this._invoiceService.GetLastMonthInvoices(monthsAgo);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentRemindList()
        {

            var paymentStatus = await this._invoiceService.GetInvoiceSellPaymentStatus();

            return Ok(paymentStatus);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> SetConfirmation(int id, [FromBody] ExtraInfoConfirmationDTO dto)
        {

            if (!ModelState.IsValid)
            {
                return NoContent();
            }

            var db = await this._invoiceService.QueryableInvoiceSell().FirstOrDefaultAsync(f => f.InvoiceSellId == id);

            if (dto.ConfirmationType == InvoiceStatusConfirm.StatusPayment)
            {
                db.PaymentDate = dto.PaymentDate;
                db.PaymentIsDone = true;
            }
            else
            {

                var extraInfo = db.ExtraInfo ?? new InvoiceExtraInfo();
                var newExtraChecked = new InvoiceExtraInfoChecked();
                newExtraChecked.Checked = true;
                newExtraChecked.Date = dto.PaymentDate;
                newExtraChecked.Info = dto.Info;

                this._db.Entry(newExtraChecked).State = EntityState.Added;

                if (dto.ConfirmationType == InvoiceStatusConfirm.StatusCMR)
                {
                    newExtraChecked.CmrChecked = extraInfo;

                }
                else if (dto.ConfirmationType == InvoiceStatusConfirm.StatusInvoiceSent)
                {
                    newExtraChecked.SentChecked = extraInfo;

                }
                else if (dto.ConfirmationType == InvoiceStatusConfirm.StatusInvoiceReceived)
                {
                    newExtraChecked.RecivedChecked = extraInfo;
                }



                if (db.ExtraInfo == null)
                {
                    this._db.Entry(extraInfo).State = EntityState.Added;

                }
                else
                {
                    this._db.Entry(extraInfo).State = EntityState.Modified;
                }

            }





            await this._db.SaveChangesAsync();


            // return clean paymentStatus list
            var invoiceSellPaymentStatus = await this._invoiceService.GetInvoiceSellPaymentStatus();
            return Ok(invoiceSellPaymentStatus);

        }

        [HttpPost]
        public IActionResult PostCalcRates([FromBody] InvoiceSellDTO inv)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(inv);
        }

        [HttpPost]
        public IActionResult PostCalcLineGroup([FromBody] InvoiceLinesGroupDTO lineGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            this._invoiceService.CalcInvoiceLineDTO(lineGroup.Current);

            return Ok(lineGroup);
        }


        [AllowAnonymous]
        [HttpGet("{dateStart}/{dateEnd}/{celZlozenia?}")]
        public async Task<IActionResult> GetJpk(DateTime dateStart, DateTime dateEnd, int celZlozenia)
        {
            var dateRange = new DateRangeDTO
            {
                DateEnd = dateEnd,
                DateStart = dateStart
            };

            var res = await this._invoiceService.GetJpk(dateRange);


            //ContentResult result = new ContentResult();
            //result.ContentType = "application/xml";
            //result.Content = res.ConvertToStringBuilder().ToString();
            //result.StatusCode = 200;

            //return result;


            return File(new System.Text.UTF8Encoding().GetBytes(res.ConvertToStringBuilder().ToString()), "text/csv", "export.csv");

            //return Ok(res);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] InvoiceSellDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbInvoice = new InvoiceSell();
            if (id > 0)
            {
                dbInvoice = await this._invoiceService.QueryableInvoiceSell()
                    .FirstOrDefaultAsync(f => f.InvoiceSellId == id);

                if (dbInvoice == null)
                {
                    return BadRequest(bp.sharedLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono faktury o Id: {id}"));
                }
            }
            else
            {
                this._db.Entry(dbInvoice).State = EntityState.Added;
            }

            //correction - create NEW 
            if (dto.IsCorrection && dto.InvoiceNo == null)
            {
                //setting inActive
                var newInvCorr = new InvoiceSell();
                this._invoiceService.InvoiceSellCorrectionSetInactive(dbInvoice);

                newInvCorr.BaseInvoiceId = dbInvoice.InvoiceSellId;

                await this._invoiceService.MapperInvoiceSell(newInvCorr, dto, User);
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

                var correctedDTO = this._invoiceService.EtoDTOInvoiceSellForInvoiceCorrection(this._invoiceService.EtoDTOInvoiceSell(newInvCorr), this._invoiceService.EtoDTOInvoiceSell(dbInvoice));
                return Ok(correctedDTO);
            }

            if (dto.IsCorrection && dto.InvoiceNo != null)
            {
                if (dbInvoice.IsInactive == false)
                {
                    await this._invoiceService.MapperInvoiceSell(dbInvoice, dto, User);
                    await this._db.SaveChangesAsync();
                }

                var baseInvoice = await this._invoiceService.QueryableInvoiceSell()
                    .FirstOrDefaultAsync(f => f.InvoiceSellId == dbInvoice.BaseInvoiceId.Value);
                return Ok(this._invoiceService.EtoDTOInvoiceSellForInvoiceCorrection(this._invoiceService.EtoDTOInvoiceSell(dbInvoice), this._invoiceService.EtoDTOInvoiceSell(baseInvoice)));
            }

            if (dbInvoice.IsInactive == false)
            {
                await this._invoiceService.MapperInvoiceSell(dbInvoice, dto, User);
            }

            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }


            return Ok(this._invoiceService.EtoDTOInvoiceSell(dbInvoice));
        }

        [HttpPost]
        public async Task<IActionResult> GenInvoicePdf([FromBody] InvoiceSellDTO invoiceSell)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            invoiceSell.CompanySeller = await this._companyService.OwnerDTO();
            if (invoiceSell.CompanyBuyer.AddressList.Count == 0 || invoiceSell.CompanyBuyer.BankAccountList.Count == 0)
            {
                invoiceSell.CompanyBuyer = this._companyService.GetCompanyDTOById((int)invoiceSell.CompanyBuyer.CompanyId.Value);
            }
            MemoryStream ms = new MemoryStream(_pdf.InvoicePdf(invoiceSell).ToArray());
            return File(ms, "application/pdf", "invoice.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> PostCloneGroup([FromBody] InvoiceSellGroupClone payload)
        {
            var start = DateTime.Now;
            await this._invoiceService.InvoiceSellGroupClone(payload, User);
            var end = DateTime.Now;
            var p = this.Request.Path.Value.ToString();
            var info = $"Zapisane, dane operacji: {start} - { end}, {end - start}";
            return Ok();
        }

    }


}

