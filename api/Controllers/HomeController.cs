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
using Microsoft.Extensions.Options;
using bp.Pomocne.DTO;
using Microsoft.AspNetCore.Hosting;

namespace api.Controllers
{
    public class HomeController : Controller
    {

        public HomeController(IOptions<ConfigurationDTO> opt, IHostingEnvironment env)
        {
            this.Opt = opt;
            this.Env = env;
        }

        private readonly IOptions<ConfigurationDTO> Opt;
        private readonly IHostingEnvironment Env;


        public IActionResult Index()
        {

            ViewBag.Env = this.Env.EnvironmentName;

            return View();
        }


        [Authorize]
        public IActionResult getDupa()
        {
            return Ok(DateTime.Now);
        }

        //public IActionResult About()
        //{
        //    ViewData["Message"] = "Your application description page.";

        //    return View();
        //}

        //public IActionResult Contact()
        //{
        //    ViewData["Message"] = "Your contact page.";

        //    return View();
        //}

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
