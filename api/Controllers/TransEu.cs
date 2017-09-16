using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Controllers
{
    public class TransEu:Controller
    {

        [Authorize(ActiveAuthenticationSchemes ="")]
        [HttpGet]
        public IActionResult GetTransEuAppCreds()
        {
            return Ok(new
            {
                ClientId = "1500452797ugJUaJngh1BxIK5Y0DX5",
                Client_secret = "StU1S3hHOCZTDtoV6sQSavdkziIUfuvL9E4FMWa4"
            });
        }
    }
}
