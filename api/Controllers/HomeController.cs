using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using api.Models.AccountViewModels;
using Microsoft.Extensions.Configuration;

namespace api.Controllers
{
    public class HomeController : Controller
    {

        private readonly IConfiguration conf;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult GetDupa() {

            var result = bp.Pomocne.DocumentNumbers.DocNumbersMethods.ParseDocNumberShort("232/03/13");

            return Ok(result);
        }

    }
}
