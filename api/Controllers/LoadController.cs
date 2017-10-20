using bp.ot.s.API.Models.Load;
using bp.PomocneLocal.Pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace bp.ot.s.API.Controllers
{
    [Route("api/[controller]/[action]")]
    public class LoadController:Controller
    {
        private IHostingEnvironment _env;
        private readonly PdfRaports _pdf;

        public LoadController(IHostingEnvironment env, PdfRaports pdf)
        {
            this._env = env;
            this._pdf = pdf;
        }

        [HttpPost]
        public IActionResult GetOrder([FromBody] LoadDTO loadDTO)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var pdfMs = _pdf.LoadOfferPdf(loadDTO);
            MemoryStream ms = new MemoryStream(pdfMs.ToArray());
            return File(ms, "application/pdf", "raport.pdf");

        }

    }

    public class Datemf
    {
        public DateTime Date { get; set; }
    }
}
