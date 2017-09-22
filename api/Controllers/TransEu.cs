using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using bp.ot.s.API.Entities.Context;
using api.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace bp.ot.s.API.Controllers
{
    [Authorize]
    public class TransEu : Controller
    {
        private readonly string _transEuAuthUrl;
        private readonly OfferTransDbContextIdent _identContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransEu(IConfiguration config, OfferTransDbContextIdent identContext, UserManager<ApplicationUser> userManager )
        {
            this._transEuAuthUrl = config["TransEu:url"];
            this._identContext = identContext;
            this._userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransEuAppCreds()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(userId);


            if (user!=null && !string.IsNullOrEmpty(user.TransId) && String.IsNullOrEmpty(user.TransUserSecret)) {

            } 


            var req = new HttpRequestMessage(HttpMethod.Post, _transEuAuthUrl)
            {
                Content = new FormUrlEncodedContent(this.TokenCredentials(user.TransId, user.TransUserSecret)),
            };

            var httpClient = new HttpClient();
            var result = await httpClient.SendAsync(req);

            if (result.IsSuccessStatusCode) {
                Console.WriteLine(result.Content.ReadAsStringAsync());
                return Ok(result.Content.ReadAsStringAsync().Result);
            }


            return Ok(new
            {
                token = "",
                ClientId = "1500452797ugJUaJngh1BxIK5Y0DX5",
                Client_secret = "StU1S3hHOCZTDtoV6sQSavdkziIUfuvL9E4FMWa4"
            });
        }


#region helpful
        private List<KeyValuePair<string, string>> TokenCredentials(string userName, string password){
            var dataKeyValue = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", "1500452797ugJUaJngh1BxIK5Y0DX5"),
                new KeyValuePair<string, string>("client_secret", "StU1S3hHOCZTDtoV6sQSavdkziIUfuvL9E4FMWa4"),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("scope", "offers.loads.manage"),
                new KeyValuePair<string, string>("username", "905754-3")
            };
            return dataKeyValue;
        }

    }







}
#endregion