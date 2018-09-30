using bp.kpir.DAO.Invoice;
using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Services;
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
            //dateEnd = bp.shared.DateHelp.DateHelpful.DateRangeDateTo(dateEnd);
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

        [HttpGet]
        public async Task<IActionResult> GetPaymentRemindList()
        {
            var dbRes = await this._invoiceService.QueryableInvoiceSell()
                .Where(w => w.IsInactive == false && w.PaymentIsDone == false)
                .ToListAsync();

            var dbCorrs = await this._invoiceService.QueryableInvoiceSell()
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

                var dto = this._invoiceService.EtoDTOInvoiceSell(inv);
                if (inv.IsCorrection)
                {
                    dto = this._invoiceService.EtoDTOInvoiceSellForInvoiceCorrection(dto, this._invoiceService.EtoDTOInvoiceSell(dbCorrs.FirstOrDefault(f => f.InvoiceSellId == dto.BaseInvoiceId)));
                }

                var payToAdd = new InvoicePaymentRemindDTO();
                var rnd = new InvoicePaymentRemindDTO();

                rnd.Company = this._companyService.CompanyCardMapper(inv.Buyer);
                rnd.Currency = dto.Currency;
                rnd.InvoiceId = dto.InvoiceSellId;
                rnd.InvoiceNo = dto.IsCorrection ? "Faktura korygująca: " + dto.InvoiceNo : "Faktura VAT: " + dto.InvoiceNo;
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
                            else
                            {
                                unpaid.Add(rnd);
                            }
                        }
                        else
                        {
                            //not confirmed recived
                            rnd.PaymentDate = inv.SellingDate.AddDays(inv.PaymentTerms.PaymentDays.Value);
                            notConfirmed.Add(rnd);
                        }
                    }
                    else
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
                UnpaidOverdue = unpaidOverdue.OrderBy(o => o.PaymentDate).ToList(),
                UnpaidOverdueStats = unpaidOverdueStats,
                NotConfirmed = notConfirmed.OrderBy(o => o.PaymentDate).ToList(),
                NotConfirmedStats = notConfirmedStats
            };



            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentRemindByDateList()
        {
            var payments = await this._invoiceService.InvoiceSellPaymentRemindList();
            var res = payments.GroupBy(g => g.PaymentDate)
                .Select(s => new
                {
                    PaymentDay = s.Key,
                    Payments = s.ToList()
                });


            return Ok(res);
        }

        [HttpGet("{id}/{paymentDate}")]
        public async Task<IActionResult> SetPaymentConfirmation(int id, DateTime paymentDate)
        {
            var inv = await this._db.InvoiceSell.FirstOrDefaultAsync(f => f.InvoiceSellId == id);
            inv.PaymentDate = paymentDate;
            inv.PaymentIsDone = true;

            await this._db.SaveChangesAsync();
            return NoContent();
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


            return File(new System.Text.UTF8Encoding().GetBytes(res.ConvertToStringBuilder().ToString()),"text/csv", "export.csv");

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
            MemoryStream ms = new MemoryStream(_pdf.InvoicePdf(invoiceSell).ToArray());
            return File(ms, "application/pdf", "invoice.pdf");
        }

    }
}

