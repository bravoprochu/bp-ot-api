using api.Models;
using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Models.TransEu;
using bp.shared.ErrorsHelper;
using bp.sharedLocal.ModelStateHelpful;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace bp.ot.s.API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Spedytor")]
    [Route("api/[controller]/[action]")]
    public class TransEu : Controller
    {
        private readonly string _transEuAuthUrl;
        private readonly BpKpirContextDane _identContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private List<TransEuAccessCredentialsDTO> _tokenAccessList;
        private ContextErrorHelper _contextErrorHelper;

        public TransEu(IConfiguration config, BpKpirContextDane identContext, UserManager<ApplicationUser> userManager, List<TransEuAccessCredentialsDTO> transEuCredentials, ContextErrorHelper errorHelper)
        {
            this._transEuAuthUrl = config["TransEu:url"];
            this._identContext = identContext;
            this._userManager = userManager;
            this._tokenAccessList = transEuCredentials;
            this._contextErrorHelper = errorHelper;
        }

        [HttpPost]
        public async Task<IActionResult> CompanyEmployeeList([FromBody] TransEuCompanyEmployeeUrlDTO url)
        {
            var link = new Uri(url.EmployeeUrl);

            var response = await this.TransEuHttpResponseMessage(HttpMethod.Get, link);
            if (response != null)
            {
                return Ok(response);
            }

            return BadRequest(ModelStateHelpful.ModelError(this._contextErrorHelper.ContextErrorCollection));

        }


        [HttpGet("{id}")]
        public async Task<IActionResult> KontrahentById(string id)
        {

            if (string.IsNullOrWhiteSpace(id)) return BadRequest(ModelStateHelpful.ModelError("KontrahentId", "Przesłana wartość: kontrahentId nie może być pusta"));

            var transEuKontrahentUri = new Uri("https://companies.system.trans.eu/api/rest/v1/companies/" + id);

            var response = await this.TransEuHttpResponseMessage(HttpMethod.Get, transEuKontrahentUri);

            if (response != null) {
                return Ok(response);
            } 
            return BadRequest(ModelStateHelpful.ModelError(this._contextErrorHelper.ContextErrorCollection));
        }



        public async Task<IActionResult> Loads()
        {
            // string query = @"?filter={""price_currency"":""PLN""}&sort={""creation_date"": 1}";
            string q1 = @"?sort={""creation_date"": -1}";

            var loadsUrl = new Uri("https://offers.system.trans.eu/api/rest/v1/loads"+q1);
            

            var response = await this.TransEuHttpResponseMessage(HttpMethod.Get, loadsUrl);

            if (response != null)
            {
                return Ok(response);
            }
            return BadRequest(ModelStateHelpful.ModelError(this._contextErrorHelper.ContextErrorCollection));
        }







            #region helpful
            private FormUrlEncodedContent TokenFormUrlEncoded(ApplicationUser user) {
                var dataKeyValue = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", "1500452797ugJUaJngh1BxIK5Y0DX5"),
                new KeyValuePair<string, string>("client_secret", "StU1S3hHOCZTDtoV6sQSavdkziIUfuvL9E4FMWa4"),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("password", user.TransUserSecret),
                new KeyValuePair<string, string>("scope", "offers.loads.manage companies.employees.me.read companies.companies.me.read companies.companies.employees.me.read exchange-transactions.load-transactions.company-offerer.read exchange-transactions.load-transactions.company-contractor.read exchange-transactions.vehicle-transactions.company-offerer.read exchange-transactions.vehicle-transactions.company-contractor.read orders.orders.basic.read orders.shipping-orders.basic.create tfs.dedicated-orders.basic.read tfs.dedicated-orders.basic.create"),
                new KeyValuePair<string, string>("username", user.TransId)
            };

                var formUrlEncoded = new FormUrlEncodedContent(dataKeyValue);
                return formUrlEncoded;
            }

            private async Task<TransEuTokenDTO> GetTransEuTokenAsync() {
                var userName = GetUserName();
                //check local cache for active access token
                if (this.IsAcctiveTokenOnList(userName))
                {
                    return this.GetActiveTokenFromListByUser(userName).Token;
                }

                //get remoteToken
                var user = await this.GetUserWithTransEuCreds();
                if (user == null)
                {
                    this._contextErrorHelper.AddError("GetToken", "GetUserError");
                    return null;
                }

                    var httpClient = new HttpClient();
                    var tokenUri = new Uri("https://auth.system.trans.eu/oauth2/token");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tokenUri);
                    request.Content = this.TokenFormUrlEncoded(user);
                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode) {
                        var token = await response.Content.ReadAsStringAsync();
                        TransEuTokenDTO accessToken = Newtonsoft.Json.JsonConvert.DeserializeObject<TransEuTokenDTO>(token);

                    //set token to local cache

                    AddActiveTokenToList(accessToken, user);
                        return accessToken;
                    } else
                    {
                        var errInfo = await response.Content.ReadAsStringAsync();
                        dynamic jsonErr = Newtonsoft.Json.JsonConvert.DeserializeObject(errInfo);
                        throw new HttpRequestException();
                    }
                }




            private async Task<string> TransEuHttpResponseMessage(HttpMethod method, Uri url) {

            var token = await this.GetTransEuTokenAsync();
            if (token == null) {
                this._contextErrorHelper.AddError("GetNewHttpClient", "Token is null");
            }
            var req = new HttpRequestMessage(method, url);
            req.Headers.TryAddWithoutValidation("Accept", "application / hal + json");
            req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token.Access_token);
            var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(req);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else {
                _contextErrorHelper.AddError("TransEuHttpResponseMessage", response.ReasonPhrase);
                // var responseText = await response.Content.ReadAsStringAsync();
                return null;
            }
        }

            private void AddActiveTokenToList(TransEuTokenDTO token, ApplicationUser user) {
                this._tokenAccessList.Add(new TransEuAccessCredentialsDTO
                {
                    Token = token,
                    TransId = user.TransId,
                    UserName = user.UserName
                });
            }

            private bool IsAcctiveTokenOnList(string userName)
            {
                var tokenOnList = this.GetActiveTokenFromListByUser(userName);
                if (tokenOnList != null && tokenOnList.ValidUntil < DateTime.Now) { this._tokenAccessList.Remove(tokenOnList); }
                return (tokenOnList != null && tokenOnList.ValidUntil > DateTime.Now) ? true : false;
            }

            private TransEuAccessCredentialsDTO GetActiveTokenFromListByUser(string userName)
            {
                return this._tokenAccessList.Find(f => f.UserName == userName);
            }

            
            private string GetUserName() => this.User.Identities.FirstOrDefault().Claims.FirstOrDefault().Value;

            private async Task<ApplicationUser> GetUserWithTransEuCreds()
            {
                var user = await _userManager.FindByNameAsync(GetUserName());

            if (string.IsNullOrWhiteSpace(user.TransId) || string.IsNullOrWhiteSpace(user.TransUserSecret))
            {
                _contextErrorHelper.AddError("GetUserWithTransEuCreds", "Użytkownik nie ma przypisanego właściwie konta TransEU");
                return null;
            }
            else return user;
            }

        }
    }
#endregion