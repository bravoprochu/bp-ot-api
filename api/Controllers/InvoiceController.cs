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

namespace bp.ot.s.API.Controllers
{
    [Route("api/[controller]/[action]")]
    public class InvoiceController:Controller
    {
        private readonly PdfRaports _pdf;

        public InvoiceController(PdfRaports pdf)
        {
            this._pdf = pdf;
        }

        [Route("api/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="Finanse")]
        public IActionResult Get()
        {


            return Ok("get");
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
    }
}
