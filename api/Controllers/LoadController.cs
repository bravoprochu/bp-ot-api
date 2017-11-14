using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Entities.Dane.Address;
using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Dane.Invoice;
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
using bp.Pomocne.DocumentNumbers;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using bp.Pomocne.DTO;

namespace bp.ot.s.API.Controllers
{
    [Route("api/[controller]/[action]")]
    public class LoadController : Controller
    {
        private IHostingEnvironment _env;
        private readonly PdfRaports _pdf;
        private OfferTransDbContextDane _db;
        private readonly CompanyService _companyService;
        private readonly InvoiceService _invoiceService;
        private readonly List<ViewValueDictionary> _viewValueDictionary;

        public LoadController(IHostingEnvironment env,
            PdfRaports pdf,
            OfferTransDbContextDane db,
            CompanyService companyService,
            InvoiceService invoiceService)
        {
            this._env = env;
            this._pdf = pdf;
            this._db = db;
            this._companyService = companyService;
            this._invoiceService = invoiceService;
            this._viewValueDictionary = _db.ViewValueDictionary.ToList();
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var resList = new List<LoadDTO>();

            var resSell = await this.LoadQueryable()
                .Where(w => w.LoadBuy != null && w.LoadTransEu != null && w.LoadSell != null)
                .OrderByDescending(o => o.LoadId)
                .ToListAsync();

            var resTrans = await this.LoadTranEuQueryable()
                .Where(w => w.LoadBuy != null && w.LoadTransEu != null && w.LoadSell == null)
                .OrderByDescending(o => o.LoadId)
                .ToListAsync();

            var resBuy = await this.LoadBuyQueryable()
                .Where(w => w.LoadBuy != null && w.LoadTransEu == null)
                .OrderByDescending(o => o.LoadId)
                .ToListAsync();





            foreach (var load in resSell)
            {
                resList.Add(this.EtDTOLoad(load));
            }

            foreach (var load in resBuy)
            {
                resList.Add(this.EtDTOLoad(load));
            }

            foreach (var load in resTrans)
            {
                resList.Add(this.EtDTOLoad(load));
            }


            return Ok(resList.OrderByDescending(o => o.LoadId));
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            //var isSell = await this.LoadBuyQueryable() //min include

            var isSell=await this.LoadBuyQueryable()
                .FirstOrDefaultAsync(s => s.LoadId == id);

            if (isSell == null)
            {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("", $"Nie znaleziono ładunku o ID: {id}"));
            }

            Load res = new Load();

            if (isSell.LoadSell != null)
            {
                res = await this.LoadQueryable()
                    .FirstOrDefaultAsync(f => f.LoadId == id);

                return Ok(this.EtDTOLoad(res));
            }

            if (isSell.LoadTransEu != null) {
                res = await this.LoadTranEuQueryable()
                    .FirstOrDefaultAsync(f => f.LoadId == id);

                return Ok(this.EtDTOLoad(res));
            }

            return Ok(this.EtDTOLoad(isSell));
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LoadDTO lDTO)
        {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbLoad = new Load();
            var dbLoadBuy = new LoadBuy();
            //buingInfo
            var dbBuyTi = new TradeInfo();
            await this.TradeInfoMapper(dbBuyTi, lDTO.Buy.Buying_info);
            //dbBuyTi.LoadBuy = dbLoadBuy;
            dbBuyTi.LoadBuy = dbLoadBuy;
            //              dbLoadBuy.BuyingInfo = dbBuyTi;
            this._db.Entry(dbBuyTi).State = EntityState.Added;
            //---------------------------------------

            var bliDTO = lDTO.Buy.Load_info;
            var dbLi = new LoadInfo();

            this.LoadInfoMapper(dbLi, bliDTO);
            dbLi.LoadBuy = dbLoadBuy;
            this._db.Entry(dbLi).State = EntityState.Added;

            var dbliExtra = new LoadInfoExtra();
            this.LoadInfoExtraMapper(dbliExtra, bliDTO.Extra_info);
            dbliExtra.LoadInfo = dbLi;
            //dbLi.ExtraInfo = dbliExtra;
            this._db.Entry(dbliExtra).State = EntityState.Added;

            if (lDTO.Buy.Routes.Count == 0) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("", "Nie można zapisać ładunku bez określenia tras"));
            }
            foreach (var route in lDTO.Buy.Routes)
            {
                var dbRoute = new LoadRoute();
                this.LoadRouteMapper(dbRoute, route);

                foreach (var pall in route.Pallets)
                {
                    var dbPall = new LoadRoutePallet();
                    this.LoadRoutePalletMapper(dbPall, pall);
                    dbPall.LoadRoute = dbRoute;
                    this._db.Entry(dbPall).State = EntityState.Added;
                    //dbRoute.Pallets.Add(dbPall);
                }

                dbRoute.LoadBuy = dbLoadBuy;
                this._db.Entry(dbRoute).State = EntityState.Added;
                //dbLoadBuy.Routes.Add(dbRoute);
            }
            //dbLoad.LoadBuy = dbLoadBuy;
            dbLoadBuy.Load = dbLoad;
            this._db.Entry(dbLoadBuy).State = EntityState.Added;




            // LoadSell
            //var dbLoadSell = new LoadSell();
            //var loadSellDTO = lDTO.Sell;
            ////pesonsContact
            //foreach (var person in lDTO.Sell.Contact_persons_list)
            //{
            //    var dbPerson = this._db.CompanyEmployee.Find(person.CompanyEmployeeId);
            //    if (dbPerson != null) {
            //        var loadSellContact = new LoadSellContactPersons();
            //        loadSellContact.CompanyEmployee = dbPerson;
            //        loadSellContact.LoadSell = dbLoadSell;
            //        this._db.Entry(loadSellContact).State = EntityState.Added;
            //        //dbLoadSell.ContactPersonsList.Add(dbPerson);
            //    }
            //}
            //dbLoadSell.Principal = this._db.Company.Find(loadSellDTO.Principal.CompanyId);


            //var dbSellTi = new TradeInfo();
            //await this.TradeInfoMapper(dbSellTi, lDTO.Sell.Selling_info);

            //dbSellTi.LoadSell = dbLoadSell;
            //this._db.Entry(dbSellTi).State = EntityState.Added;
            ////dbLoadSell.SellingInfo = dbSellTi;

            dbLoad.Info = lDTO.Info;
            dbLoad.LoadNo = new DocNumber().GenNumberMonthYearNumber(this._db.Load.LastOrDefault()?.LoadNo, DateTime.Now, '/').DocNumberCombined;
            //dbLoadSell.Load = dbLoad;

            this._db.Entry(dbLoad).State = EntityState.Added;

            try
            {
                this._db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }


            return Ok(dbLoad.LoadId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuy(int id, [FromBody] LoadBuyDTO lDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbLoad = new Load();
            var dbLoadBuy = new LoadBuy();

            if (id > 0)
            {
                dbLoad = await this.LoadBuyQueryable()
                    .FirstOrDefaultAsync(f => f.LoadId == id);

                if (dbLoad == null)
                {
                    return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono ładunku o Id: {id}"));
                }
                await this.UpdateLoadBuy(dbLoad.LoadBuy, lDTO);
            }

            

            if (id == 0)
            {
                await this.UpdateLoadBuy(dbLoadBuy, lDTO);
                dbLoad.LoadNo = new bp.Pomocne.DocumentNumbers.DocNumber().GenNumberMonthYearNumber(await this._db.Load.Select(s => s.LoadNo).LastOrDefaultAsync(), lDTO.Buying_info.Date, '/').DocNumberCombined;
                this._db.Entry(dbLoadBuy).State = EntityState.Added;
                dbLoad.LoadBuy = dbLoadBuy;
                this._db.Entry(dbLoad).State = EntityState.Added;
            }

            await this._db.SaveChangesAsync();


            return Ok();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransEu(int id, [FromBody] LoadDTO lDTO)
        {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbLoad = await this.LoadBuyQueryable()
                .Include(i => i.LoadTransEu)
                .FirstOrDefaultAsync(f => f.LoadId == id);

            if (dbLoad == null) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Błąd", $"Nie znaleziono ładunku o Id: {id}"));
            }


            if (dbLoad.LoadSell != null) {
                dbLoad = await this.LoadTranEuQueryable()
                    .FirstOrDefaultAsync(f => f.LoadId == id);
            }

            //update LoadBuy
            await this.UpdateLoadBuy(dbLoad.LoadBuy, lDTO.Buy);
            var dbTrans = dbLoad.LoadTransEu ?? new LoadTransEu();
            if (dbLoad.LoadTransEu == null)
            {
                await this.UpdateTransEu(dbTrans, lDTO.TransEu);
                dbTrans.Load = dbLoad;
                this._db.Entry(dbTrans).State = EntityState.Added;
            }
            else {
                await this.UpdateTransEu(dbTrans, lDTO.TransEu);
            }

            await this._db.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) {

            var dbLoad = await this._db.Load
                .FirstOrDefaultAsync(f => f.LoadId == id);

            if (dbLoad == null) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono ładunku o ID: {id}"));
            }


            //only loadBuy
            var dbLoadBuy = await this._db.LoadBuy
                .Include(i=>i.LoadInfo)
                .Include(i=>i.BuyingInfo)
                .Include(i=>i.Routes)
                .FirstOrDefaultAsync(f => f.LoadId == id);
            if (dbLoadBuy != null)
            {
                var lBuyLoadInfo = dbLoadBuy.LoadInfo;

                if (lBuyLoadInfo != null)
                {
                    var extra = dbLoad.LoadBuy.LoadInfo.ExtraInfo;
                    if (extra != null)
                    {
                        this._db.Entry(extra).State = EntityState.Deleted;
                    }
                    this._db.Entry(lBuyLoadInfo).State = EntityState.Deleted;
                }

                
                if (dbLoadBuy.BuyingInfo != null)
                {
                    var lBuyTradeInfo = await this._db.LoadTradeInfo
                        .Include(i => i.CurrencyNbp)
                        .Include(i => i.PaymentTerms)
                        .FirstOrDefaultAsync(f => f.LoadBuy.LoadId == id);

                    if (lBuyTradeInfo.PaymentTerms!= null)
                    {
                        this._db.Entry(lBuyTradeInfo.PaymentTerms).State = EntityState.Deleted;
                    }
                    this._db.Entry(lBuyTradeInfo).State = EntityState.Deleted;


                    if (lBuyTradeInfo.CurrencyNbp != null) {
                        this._db.Entry(lBuyTradeInfo.CurrencyNbp).State = EntityState.Deleted;
                    }
                }
                var routes = await this._db.LoadRoute
                    .Include(i => i.Address)
                    .Include(i => i.Pallets)
                    .Where(w => w.LoadBuy.LoadId == id)
                    .ToListAsync();

                foreach (var dbRoute in routes)
                {
                    foreach (var dbPallet in dbRoute.Pallets)
                    {
                        this._db.Entry(dbPallet).State = EntityState.Deleted;
                    }
                    this._db.Entry(dbRoute.Address).State = EntityState.Deleted;
                    this._db.Entry(dbRoute).State = EntityState.Deleted;
                }

                this._db.Entry(dbLoadBuy).State = EntityState.Deleted;
            }


            //transEu
            var dbLoadTransEu = await this._db.LoadTransEu
                .Include(i=>i.ContactPersonsList)
                .Include(i=>i.Price)
                .FirstOrDefaultAsync(f => f.LoadId == id);

            if (dbLoadTransEu != null)
            {
                if (dbLoadTransEu.Price != null)
                {
                    this._db.Entry(dbLoadTransEu.Price).State = EntityState.Deleted;
                }

                foreach (var contact in dbLoadTransEu.ContactPersonsList)
                {
                    this._db.Entry(contact).State = EntityState.Deleted;
                }

                this._db.Entry(dbLoadTransEu).State = EntityState.Deleted;
            }


            var dbLoadSell = await this._db.LoadSell
                .Include(i=>i.ContactPersonsList)
                .Include(i=>i.SellingInfo)
                .FirstOrDefaultAsync(f => f.LoadId == id);

            if (dbLoadSell != null) {
                foreach (var contact in dbLoadSell.ContactPersonsList)
                {
                    this._db.Entry(contact).State = EntityState.Deleted;
                }
                var dbTerms = await this._db.PaymentTerms.Where(w => w.TradeInfo.LoadSell.LoadId == id).FirstOrDefaultAsync();
                if (dbTerms != null) {
                    this._db.Entry(dbTerms).State = EntityState.Deleted;
                }

                
                this._db.Entry(dbLoadSell.SellingInfo).State = EntityState.Deleted;
                this._db.Entry(dbLoadSell).State = EntityState.Deleted;
            }

            this._db.Entry(dbLoad).State = EntityState.Deleted;
            await this._db.SaveChangesAsync();

            return Ok();
        }



        [HttpPost]
        public IActionResult GetOrderPdf([FromBody] LoadDTO loadDTO)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var pdfMs = _pdf.LoadOfferPdf(loadDTO);
            MemoryStream ms = new MemoryStream(pdfMs.ToArray());
            return File(ms, "application/pdf", "raport.pdf");

        }




        #region
        private IQueryable<Load> LoadBuyQueryable()
        {
            return this._db.Load
                .Include(i=>i.LoadBuy)
                .Include(i => i.LoadBuy).ThenInclude(i => i.BuyingInfo).ThenInclude(i => i.PaymentTerms).ThenInclude(i => i.PaymentTerm)
                .Include(i => i.LoadBuy).ThenInclude(i => i.BuyingInfo).ThenInclude(i => i.CurrencyNbp).ThenInclude(i => i.Currency)
                .Include(i => i.LoadBuy).ThenInclude(i => i.BuyingInfo).ThenInclude(i => i.Company).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadBuy.LoadInfo)
                .Include(i => i.LoadBuy.LoadInfo).ThenInclude(i=>i.ExtraInfo)
                .Include(i => i.LoadBuy.LoadInfo).ThenInclude(i => i.ExtraInfo).ThenInclude(i=>i.RequiredAddrClassess).ThenInclude(i=>i.ViewValueDictionary)
                .Include(i => i.LoadBuy.LoadInfo).ThenInclude(i => i.ExtraInfo).ThenInclude(i => i.RequiredTruckBody)
                .Include(i => i.LoadBuy.LoadInfo).ThenInclude(i => i.ExtraInfo).ThenInclude(i => i.RequiredWaysOfLoading).ThenInclude(i => i.ViewValueDictionary)
                .Include(i => i.LoadBuy.LoadInfo).ThenInclude(i => i.ExtraInfo).ThenInclude(i => i.TypeOfLoad)
                .Include(i => i.LoadBuy.Routes).ThenInclude(i => i.Address)
                .Include(i => i.LoadBuy.Routes).ThenInclude(i => i.Pallets)
                .Include(i => i.LoadTransEu)
                .Include(i => i.LoadSell);
        }

        private IQueryable<Load> LoadTranEuQueryable()
        {
            return this.LoadBuyQueryable()
                .Include(i => i.LoadTransEu.ContactPersonsList).ThenInclude(i => i.CompanyEmployee)
                .Include(i => i.LoadTransEu.Price)
                .Include(i => i.LoadTransEu.SellingCompany).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadTransEu.SellingCompany).ThenInclude(i => i.EmployeeList);
        }

        private IQueryable<Load> LoadQueryable()
        {
            return this.LoadTranEuQueryable()
                .Include(i => i.LoadSell.ContactPersonsList).ThenInclude(i => i.CompanyEmployee)
                .Include(i => i.LoadSell.Principal).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadSell.Principal).ThenInclude(i => i.EmployeeList)
                .Include(i => i.LoadSell.SellingInfo.PaymentTerms.PaymentTerm)
                .Include(i => i.LoadSell.SellingInfo.Company).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadSell.SellingInfo.CurrencyNbp.Currency);
        }


        public LoadDTO EtDTOLoad(Load dbLoad)
        {
            var res = new LoadDTO();
            var lb = new LoadBuyDTO();
            var ls = new LoadSellDTO();
            var lt = new LoadTransEuDTO();


            lb.Buying_info = this.EtDTOTradeInfo(dbLoad.LoadBuy.BuyingInfo);
            lb.Load_info = this.EtDTOLoadInfo(dbLoad.LoadBuy.LoadInfo);
            lb.Routes = new List<LoadRouteDTO>();
            foreach (var route in dbLoad.LoadBuy.Routes)
            {
                lb.Routes.Add(this.EtDTOLoadRoutes(route));
            }


            if (dbLoad.LoadTransEu != null) {
                lt.LoadTransEuId = dbLoad.LoadTransEu.LoadTransEuId;
                lt.ContactPersonsList = new List<CompanyEmployeeDTO>();
                foreach (var contact in dbLoad.LoadTransEu.ContactPersonsList)
                {
                    lt.ContactPersonsList.Add(this._companyService.EtDTOEmployee(contact.CompanyEmployee));
                }
                lt.Price = this._invoiceService.EtDTOCurrencyNbp(dbLoad.LoadTransEu.Price);
                lt.SellingCompany = this._companyService.EtDTOCompany(dbLoad.LoadTransEu.SellingCompany);
                lt.TransEuId = dbLoad.LoadTransEu.TransEuId;

                res.TransEu = lt;
            }

            if (dbLoad.LoadSell != null)
            {

                ls.Contact_persons_list = new List<CompanyEmployeeDTO>();
                foreach (var person in dbLoad.LoadSell.ContactPersonsList)
                {
                    ls.Contact_persons_list.Add(this._companyService.EtDTOEmployee(person.CompanyEmployee));
                }
                ls.Principal = this._companyService.EtDTOCompany(dbLoad.LoadSell.Principal);
                ls.Selling_info = this.EtDTOTradeInfo(dbLoad.LoadSell.SellingInfo);

                res.Sell = ls;
            }

            res.LoadId = dbLoad.LoadId;
            res.Info = dbLoad.Info;
            res.LoadNo = dbLoad.LoadNo;
            res.Buy = lb;

            res.LoadNo = dbLoad.LoadNo;



            return res;
        }
        public LoadInfoDTO EtDTOLoadInfo(LoadInfo lInfo)
        {
            var res = new LoadInfoDTO();
            res.Description = lInfo.Description;
            res.Extra_info = this.EtDTOLoadInfoExtra(lInfo.ExtraInfo);
            res.Load_height = lInfo.LoadHeight;
            res.Load_length = lInfo.LoadLength;
            res.Load_volume = lInfo.LoadVolume;
            res.Load_weight = lInfo.LoadWeight;


            return res;
        }

        public LoadInfoExtraDTO EtDTOLoadInfoExtra(LoadInfoExtra eInfoExtra)
        {
            var res = new LoadInfoExtraDTO();
            res.Is_for_clearence = eInfoExtra.IsForClearence ?? false;
            res.Is_lift_required = eInfoExtra.IsLiftRequired ?? false;
            res.Is_ltl = eInfoExtra.IsLtl ?? false;
            res.Is_tir_cable_required = eInfoExtra.IsTirCableRequired ?? false;
            res.Is_tracking_system_required = eInfoExtra.IsTrackingSystemRequired ?? false;
            res.Is_truck_crane_required = eInfoExtra.IsTruckCraneRequired ?? false;
            res.Required_adr_classes = new List<Pomocne.DTO.ValueViewValueDTO>();
            if (eInfoExtra.RequiredAddrClassess != null)
            {
                foreach (var addr in eInfoExtra.RequiredAddrClassess)
                {
                    var found = this._viewValueDictionary.Where(w => w.ViewValueDictionaryId == addr.ViewValueDictionaryId).FirstOrDefault();
                    if (found != null)
                    {
                        res.Required_adr_classes.Add(this.EtDTOValueViewValue(found));
                    }
                }
            }
            if (eInfoExtra.RequiredTruckBody != null)
            {
                res.Required_truck_body = this.EtDTOValueViewValue(eInfoExtra.RequiredTruckBody);
            }
            res.Required_ways_of_loading = new List<ValueViewValueDTO>();
            if (eInfoExtra.RequiredWaysOfLoading != null)
            {
                foreach (var way in eInfoExtra.RequiredWaysOfLoading)
                {
                    var found = this._viewValueDictionary.Where(w => w.ViewValueDictionaryId == way.ViewValueDictionaryId).FirstOrDefault();
                    if (found != null)
                    {
                        res.Required_ways_of_loading.Add(this.EtDTOValueViewValue(found));
                    }
                }
            }
            if (eInfoExtra.TypeOfLoad != null)
            {
                res.Type_of_load = this.EtDTOValueViewValue(eInfoExtra.TypeOfLoad);
            }

            return res;
        }

        public LoadRouteDTO EtDTOLoadRoutes(LoadRoute route) {
            var res = new LoadRouteDTO();
            res.Address = this._companyService.EtDTOAddress(route.Address);
            res.Geo = new GeoDTO();
            res.Info = route.Info;
            res.Loading_date = route.LoadingDate;
            res.Loading_type = route.IsLoadingType ? "Załadunek" : "Rozładunek";
            res.Pallets = new List<LoadRoutePalletDTO>();
            if (route.Pallets != null)
            {
                foreach (var pall in route.Pallets)
                {
                    res.Pallets.Add(this.EtDTOLoadRoutePallet(pall));
                }
            }


            return res;
        }

        public LoadRoutePalletDTO EtDTOLoadRoutePallet(LoadRoutePallet pallet)
        {
            var res = new LoadRoutePalletDTO();
            res.Amount = pallet.Amount;
            res.Dimmension = pallet.Dimmension;
            res.Info = pallet.Info;
            res.Is_exchangeable = pallet.IsExchangeable;
            res.Is_stackable = pallet.IsExchangeable;
            res.Type = pallet.IsEuroType ? "EURO" : "Inne";
            return res;
        }

        public ValueViewValueDTO EtDTOValueViewValue(ViewValueDictionary val)
        {
            var res = new ValueViewValueDTO();
            if (val == null) { return res; }
            res.Value = val.Value;
            res.ViewValue = val.ViewValue;
            res.ViewValueDictionaryId = val.ViewValueDictionaryId;
            res.ViewValueGroupNameId = val.ViewValueGroupNameId;
            return res;
        }

        public TradeInfoDTO EtDTOTradeInfo(TradeInfo tInfo)
        {
            var res = new TradeInfoDTO();
            res.Company = this._companyService.EtDTOCompany(tInfo.Company);
            res.Date = tInfo.Date;
            res.Payment_terms = this._invoiceService.EtDTOPaymentTerms(tInfo.PaymentTerms);
            res.Price = this._invoiceService.EtDTOCurrencyNbp(tInfo.CurrencyNbp);
            res.TradeInfoId = tInfo.TradeInfoId;
            return res;
        }

        public void LoadInfoMapper(LoadInfo dbLoadInfo, LoadInfoDTO liDTO)
        {
            var dbExtraInfo = dbLoadInfo.ExtraInfo ?? new LoadInfoExtra();
            this.LoadInfoExtraMapper(dbExtraInfo, liDTO.Extra_info);
            if (dbLoadInfo.ExtraInfo == null) {
                dbExtraInfo.LoadInfo = dbLoadInfo;
                this._db.Entry(dbExtraInfo).State = EntityState.Added;
            }

            dbLoadInfo.Description = liDTO.Description;
            dbLoadInfo.LoadHeight = liDTO.Load_height;
            dbLoadInfo.LoadLength = liDTO.Load_length;
            dbLoadInfo.LoadVolume = liDTO.Load_volume;
            dbLoadInfo.LoadWeight = liDTO.Load_weight;
        }

        public void LoadInfoExtraMapper(LoadInfoExtra dbLie, LoadInfoExtraDTO lieDTO)
        {
            dbLie.IsForClearence = lieDTO.Is_for_clearence;
            dbLie.IsLiftRequired = lieDTO.Is_lift_required;
            dbLie.IsLtl = lieDTO.Is_ltl;
            dbLie.IsTirCableRequired = lieDTO.Is_tir_cable_required;
            dbLie.IsTrackingSystemRequired = lieDTO.Is_tracking_system_required;
            dbLie.IsTruckCraneRequired = lieDTO.Is_truck_crane_required;

            //truck body 
            var truckBodyDTO = lieDTO.Required_truck_body!=null ? this._viewValueDictionary.FirstOrDefault(f => f.ViewValueDictionaryId == lieDTO.Required_truck_body.ViewValueDictionaryId) : null;
            dbLie.RequiredTruckBody = truckBodyDTO;


            //addrClasses
            // remove deleted
            if (dbLie.RequiredAddrClassess != null)
            {
                foreach (var dbAdd in dbLie.RequiredAddrClassess)
                {
                    var addDTOFound = lieDTO.Required_adr_classes.FirstOrDefault(f => f.ViewValueDictionaryId == dbAdd.ViewValueDictionaryId);
                    if (addDTOFound == null)
                    {
                        this._db.Entry(dbAdd).State = EntityState.Deleted;
                    }
                }
            }

                //modify or Add
                foreach (var addDTO in lieDTO.Required_adr_classes)
                {
                    var addressDTO = this._viewValueDictionary.FirstOrDefault(f => f.ViewValueDictionaryId == addDTO.ViewValueDictionaryId);
                    var dbAddFound = dbLie.RequiredAddrClassess?.FirstOrDefault(f => f.ViewValueDictionaryId == addDTO.ViewValueDictionaryId);
                    if (dbAddFound == null)
                    {
                        var dbNewAdd = new LoadInfoExtraAddrClassess();
                        dbNewAdd.LoadInfoExtra = dbLie;
                        dbNewAdd.ViewValueDictionary = addressDTO;
                        this._db.Entry(dbNewAdd).State = EntityState.Added;
                    }
                    else
                    {
                        dbAddFound.ViewValueDictionary = addressDTO;
                    }
                }

            //ways of load
            //remove deleted
            if (dbLie.RequiredWaysOfLoading != null)
            {
                foreach (var dbWay in dbLie.RequiredWaysOfLoading)
                {
                    var wayDTO = lieDTO.Required_ways_of_loading.FirstOrDefault(f => f.ViewValueDictionaryId == dbWay.ViewValueDictionaryId);
                    if (wayDTO == null)
                    {
                        this._db.Entry(dbWay).State = EntityState.Deleted;
                    }
                }
            }

                foreach (var wayDTO in lieDTO.Required_ways_of_loading)
                {
                    var wayOfLoadDTO = this._viewValueDictionary.Find(f => f.ViewValueDictionaryId == wayDTO.ViewValueDictionaryId);
                    var dbWayFound = dbLie.RequiredWaysOfLoading?.FirstOrDefault(f => f.ViewValueDictionaryId == wayDTO.ViewValueDictionaryId);
                    if (dbWayFound == null)
                    {
                        var dbWaysOfLoad = new LoadInfoExtraWaysOfLoad();
                        dbWaysOfLoad.LoadInfoExtra = dbLie;
                        dbWaysOfLoad.ViewValueDictionary = wayOfLoadDTO;
                        this._db.Entry(dbWaysOfLoad).State = EntityState.Added;
                    }
                    else
                    {
                        dbWayFound.ViewValueDictionary = wayOfLoadDTO;
                    }
                }

            dbLie.TypeOfLoad = this._viewValueDictionary.Find(f => f.ViewValueDictionaryId == lieDTO.Type_of_load.ViewValueDictionaryId);
        }

        public void LoadRouteMapper(LoadRoute dbRoute, LoadRouteDTO routeDTO)
        {
            //            var dbGeo = dbRoute.Geo ?? new LoadRouteGeo();
            //            dbGeo.Latitude = routeDTO.Geo.Latitude;
            //            dbGeo.Longitude = routeDTO.Geo.Longitude;
            ////            this._db.Entry(dbGeo).State = dbGeo == null ? EntityState.Added : EntityState.Modified;
            //            dbRoute.Geo = dbGeo;

            var dbAddress = dbRoute.Address ?? new Address();
            this._companyService.AddresMapperDTO(dbAddress, routeDTO.Address);

            //dbAddress.LoadRoute = dbRoute;
            if (dbRoute.Address == null)
            {
                dbRoute.Address = dbAddress;
                this._db.Entry(dbAddress).State = EntityState.Added;
            }
            else {
                this._db.Entry(dbAddress).State = EntityState.Modified;
            }
            dbRoute.Info = routeDTO.Info;
            dbRoute.LoadingDate = routeDTO.Loading_date;
            dbRoute.IsLoadingType = routeDTO.Loading_type == "Załadunek" ? true : false;

            //remove deleted pallets
            foreach (var dbPal in dbRoute.Pallets)
            {
                var foundDTO = routeDTO.Pallets.FirstOrDefault(f => f.LoadRoutePalletId == dbPal.LoadRoutePalletId);
                if (foundDTO == null) {
                    this._db.Entry(foundDTO).State = EntityState.Deleted;
                }
            }
            //modifyOrAdd
            foreach (var palDTO in routeDTO.Pallets)
            {
                var foundDb = dbRoute.Pallets.FirstOrDefault(f => f.LoadRoutePalletId == palDTO.LoadRoutePalletId);
                if (foundDb == null)
                {
                    //add new
                    var newPallet = new LoadRoutePallet();
                    this.LoadRoutePalletMapper(newPallet, palDTO);
                    newPallet.LoadRoute = dbRoute;
                    this._db.Entry(newPallet).State = EntityState.Added;
                }
                else {
                    this.LoadRoutePalletMapper(foundDb, palDTO);
                }
            }
        }

        public void LoadRoutePalletMapper(LoadRoutePallet dbPallet, LoadRoutePalletDTO pallDTO)
        {
            dbPallet.Amount = pallDTO.Amount;
            dbPallet.Dimmension = pallDTO.Dimmension;
            dbPallet.Info = pallDTO.Info;
            dbPallet.IsExchangeable = pallDTO.Is_exchangeable;
            dbPallet.IsStackable = pallDTO.Is_stackable;
            dbPallet.IsEuroType = pallDTO.Type == "EURO" ? true : false;
        }


        public async Task TradeInfoMapper(TradeInfo dbTi, TradeInfoDTO tiDTO)
        {

            dbTi.Date = tiDTO.Date;
            //buying company

            if (dbTi.Company?.CompanyId != tiDTO.Company.CompanyId) {
                dbTi.Company = await this._db.Company.FindAsync(tiDTO.Company.CompanyId);
            }



            //currencyNBP (price)
            var buyCurrencyNbp = dbTi.CurrencyNbp ?? new CurrencyNbp();
            this._invoiceService.CurrencyNbpMapper(buyCurrencyNbp, tiDTO.Price);
            buyCurrencyNbp.TradeInfo = dbTi;
            if (dbTi.CurrencyNbp == null)
            {
                this._db.Entry(buyCurrencyNbp).State = EntityState.Added;
            }
            else {
                this._db.Entry(buyCurrencyNbp).State = EntityState.Modified;
            }

            dbTi.Date = tiDTO.Date;

            //buy paymentTerms
            var paymentTerms = dbTi.PaymentTerms ?? new PaymentTerms();
            this._invoiceService.PaymentTermsMapper(paymentTerms, tiDTO.Payment_terms);
            paymentTerms.TradeInfo = dbTi;
            if (dbTi.PaymentTerms == null)
            {
                this._db.Entry(paymentTerms).State = EntityState.Added;
            }
            else {
                this._db.Entry(paymentTerms).State = EntityState.Modified;
            }
        }

        private async Task UpdateLoadBuy(LoadBuy dbLoad, LoadBuyDTO lDTO)
        {
            //buyingInfo mod
            var buyingInfo = dbLoad.BuyingInfo ?? new TradeInfo();
            await this.TradeInfoMapper(buyingInfo, lDTO.Buying_info);
            if (dbLoad.BuyingInfo == null) {
                buyingInfo.LoadBuy = dbLoad;
                this._db.Entry(buyingInfo).State = EntityState.Added;
            }

            //loadInfo mod
            var loadinfo = dbLoad.LoadInfo ?? new LoadInfo();
            this.LoadInfoMapper(loadinfo, lDTO.Load_info);
            if (dbLoad.LoadInfo == null) {
                loadinfo.LoadBuy = dbLoad;
                this._db.Entry(loadinfo).State = EntityState.Added;
            }
            
            //routes updates
            //remove from db deleted
            foreach (var dbRoute in dbLoad.Routes)
            {
                var find = lDTO.Routes.Where(w => w.LoadRouteId == dbRoute.LoadRouteId).FirstOrDefault();
                if (find == null) {
                    this._db.Entry(dbRoute).State = EntityState.Deleted;
                }
            }
            //modify or add
            foreach (var route in lDTO.Routes)
            {
                var dbRoute = dbLoad.Routes.FirstOrDefault(f => f.LoadRouteId == route.LoadRouteId);
                if (dbRoute == null)
                {
                    var newRoute = new LoadRoute();
                    this.LoadRouteMapper(newRoute, route);
                    newRoute.LoadBuy = dbLoad;
                    this._db.Entry(newRoute).State = EntityState.Added;
                }
                else {
                    this.LoadRouteMapper(dbRoute, route);
                }
            }
        }

        private async Task UpdateTransEu(LoadTransEu dbTrans, LoadTransEuDTO tDTO)
        {
            //contactPersonsList
            //remove from db deleted
            if (dbTrans.ContactPersonsList?.Count > 0)
            {
                foreach (var contact in dbTrans.ContactPersonsList)
                {
                    var found = tDTO.ContactPersonsList.Where(w => w.CompanyEmployeeId == contact.CompanyEmployeeId).FirstOrDefault();
                    if (found == null)
                    {
                        this._db.Entry(contact).State = EntityState.Deleted;
                    }
                }
            }
            //modify or add new..
            foreach (var contactDTO in tDTO.ContactPersonsList)
            {
                var foundDb = dbTrans.ContactPersonsList?.FirstOrDefault(f => f.CompanyEmployeeId == contactDTO.CompanyEmployeeId);
                if (foundDb == null)
                {
                    //add new
                    var newContactDb = new LoadTransEuContactPerson
                    {
                        CompanyEmployee = await this._db.CompanyEmployee.FindAsync(contactDTO.CompanyEmployeeId),
                        LoadTransEu = dbTrans
                    };
                    this._db.Entry(newContactDb).State = EntityState.Added;
                }
                else {
                    //modify when companyID is different
                    if (foundDb.CompanyEmployeeId != contactDTO.CompanyEmployeeId) {
                        foundDb.CompanyEmployee = await this._db.CompanyEmployee.FindAsync(contactDTO.CompanyEmployeeId);
                    }
                }
            }

            //Price
            var dbPrice = dbTrans.Price ?? new CurrencyNbp();
            this._invoiceService.CurrencyNbpMapper(dbPrice, tDTO.Price);
            if (dbTrans.Price == null)
            {
                dbPrice.LoadTransEu = dbTrans;
                this._db.Entry(dbPrice).State = EntityState.Added;
            }

            //SellingCompany
            if (dbTrans.SellingCompany?.CompanyId != tDTO.SellingCompany.CompanyId) {
                dbTrans.SellingCompany = await this._db.Company.FindAsync(tDTO.SellingCompany.CompanyId);
            }
        }
            

#endregion

    }
}
