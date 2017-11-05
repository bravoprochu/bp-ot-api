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
    public class LoadController:Controller
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

            var res = await this.LoadQueryable()
                .OrderByDescending(o=>o.LoadId)
                .ToListAsync();

            var resList = new List<LoadDTO>();

            foreach (var load in res)
            {
                resList.Add(this.EtDTOLoad(load));
            }


            return Ok(resList);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await this.LoadQueryable()
                .SingleAsync(w => w.LoadId == id);
            if (res == null) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("", $"Nie znaleziono ładunku o ID: {id}"));
            }
            return Ok(this.EtDTOLoad(res));
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

           
            foreach (var route in lDTO.Buy.Routes)
            {
                var dbRoute = new LoadRoute();
                this.LoadRouteMapper(dbRoute, route);

                foreach (var pall in route.Pallets)
                {
                    var dbPall = new LoadRoutePallet();
                    this.LoadRoutePAlletMapper(dbPall, pall);
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
            var dbLoadSell = new LoadSell();
            var loadSellDTO = lDTO.Sell;
            //pesonsContact
            foreach (var person in lDTO.Sell.Contact_persons_list)
            {
                var dbPerson = this._db.CompanyEmployee.Find(person.CompanyEmployeeId);
                if (dbPerson != null) {
                    var loadSellContact = new LoadSellContactPersons();
                    loadSellContact.CompanyEmployee = dbPerson;
                    loadSellContact.LoadSell = dbLoadSell;
                    this._db.Entry(loadSellContact).State = EntityState.Added;
                    //dbLoadSell.ContactPersonsList.Add(dbPerson);
                }
            }
            dbLoadSell.Principal = this._db.Company.Find(loadSellDTO.Principal.CompanyId);
            
            
            var dbSellTi = new TradeInfo();
            await this.TradeInfoMapper(dbSellTi, lDTO.Sell.Selling_info);

            dbSellTi.LoadSell = dbLoadSell;
            this._db.Entry(dbSellTi).State = EntityState.Added;
            //dbLoadSell.SellingInfo = dbSellTi;

            dbLoad.Info = lDTO.Info;
            dbLoad.LoadNo = new DocNumber().GenNumberMonthYearNumber(this._db.Load.LastOrDefault()?.LoadNo, DateTime.Now, '/').DocNumberCombined;
            dbLoadSell.Load = dbLoad;
            this._db.Entry(dbLoadSell).State = EntityState.Added;
            this._db.Entry(dbLoad).State = EntityState.Added;

            try
            {
                this._db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }


            return Ok(this.EtDTOLoad(dbLoad));
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


        #region

        private IQueryable<Load> LoadQueryable()
        {
            return this._db.Load
                .Include(i => i.LoadBuy.BuyingInfo.PaymentTerms.PaymentTerm)
                .Include(i => i.LoadBuy.BuyingInfo.Company).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadBuy.BuyingInfo.CurrencyNbp.Currency)
                .Include(i => i.LoadBuy.LoadInfo.ExtraInfo.RequiredAddrClassess).ThenInclude(i => i.ViewValueDictionary)
                .Include(i => i.LoadBuy.LoadInfo.ExtraInfo.RequiredTruckBody)
                .Include(i => i.LoadBuy.LoadInfo.ExtraInfo.RequiredWaysOfLoading).ThenInclude(i => i.ViewValueDictionary)
                .Include(i => i.LoadBuy.LoadInfo.ExtraInfo.TypeOfLoad)
                .Include(i => i.LoadSell.ContactPersonsList).ThenInclude(i => i.CompanyEmployee)
                .Include(i => i.LoadSell.Principal).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadSell.Principal).ThenInclude(i => i.EmployeeList)
                .Include(i => i.LoadSell.SellingInfo.PaymentTerms.PaymentTerm)
                .Include(i => i.LoadSell.SellingInfo.Company).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadSell.SellingInfo.CurrencyNbp.Currency)
                .Include(i => i.LoadBuy.Routes).ThenInclude(i => i.Address)
                .Include(i => i.LoadBuy.Routes).ThenInclude(i => i.Pallets);
        }

        public LoadDTO EtDTOLoad(Load dbLoad)
        {
            var res = new LoadDTO();
            var lb= new LoadBuyDTO();
            var ls = new LoadSellDTO();
            lb.Buying_info = this.EtDTOTradeInfo(dbLoad.LoadBuy.BuyingInfo);
            lb.Load_info = this.EtDTOLoadInfo(dbLoad.LoadBuy.LoadInfo);
            lb.Routes = new List<LoadRouteDTO>();
            foreach (var route in dbLoad.LoadBuy.Routes)
            {
                lb.Routes.Add(this.EtDTOLoadRoutes(route));
            }

            ls.Contact_persons_list = new List<CompanyEmployeeDTO>();
            foreach (var person in dbLoad.LoadSell.ContactPersonsList)
            {
                ls.Contact_persons_list.Add(this._companyService.EtDTOEmployee(person.CompanyEmployee));
            }
            ls.Principal = this._companyService.EtDTOCompany(dbLoad.LoadSell.Principal);
            ls.Selling_info = this.EtDTOTradeInfo(dbLoad.LoadSell.SellingInfo);


            res.LoadId = dbLoad.LoadId;
            res.Info = dbLoad.Info;
            res.LoadNo = dbLoad.LoadNo;
            res.Buy = lb;
            res.Sell = ls;
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
            res.Is_for_clearence = eInfoExtra.IsForClearence;
            res.Is_lift_required = eInfoExtra.IsLiftRequired;
            res.Is_ltl = eInfoExtra.IsLtl;
            res.Is_tir_cable_required = eInfoExtra.IsTirCableRequired;
            res.Is_tracking_system_required = eInfoExtra.IsTrackingSystemRequired;
            res.Is_truck_crane_required = eInfoExtra.IsTruckCraneRequired;
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
            res.Required_truck_body = this.EtDTOValueViewValue(eInfoExtra.RequiredTruckBody);
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
            res.Type_of_load = this.EtDTOValueViewValue(eInfoExtra.TypeOfLoad);

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


        public void LoadInfoMapper(LoadInfo dbLi, LoadInfoDTO liDTO)
        {
            dbLi.Description = liDTO.Description;
            dbLi.LoadHeight = liDTO.Load_height;
            dbLi.LoadLength = liDTO.Load_length;
            dbLi.LoadVolume = liDTO.Load_volume;
            dbLi.LoadWeight = liDTO.Load_weight;
        }

        public void LoadInfoExtraMapper(LoadInfoExtra dblie, LoadInfoExtraDTO lieDTO)
        {
            dblie.IsForClearence = lieDTO.Is_for_clearence;
            dblie.IsLiftRequired = lieDTO.Is_lift_required;
            dblie.IsLtl = lieDTO.Is_ltl;
            dblie.IsTirCableRequired = lieDTO.Is_tir_cable_required;
            dblie.IsTrackingSystemRequired = lieDTO.Is_tracking_system_required;
            dblie.IsTruckCraneRequired = lieDTO.Is_truck_crane_required;
            var truckBodyDTO= this._viewValueDictionary.Where(w => w.ViewValueDictionaryId == lieDTO.Required_truck_body.ViewValueDictionaryId).FirstOrDefault();
            if (truckBodyDTO!=null) {
                dblie.RequiredTruckBody = truckBodyDTO;
                //this._db.Entry(truckBodyDTO).State = EntityState.Added;
            }
            //addrClasses
            dblie.RequiredAddrClassess = new List<LoadInfoExtraAddrClassess>();
            foreach (var addr in lieDTO.Required_adr_classes)
            {
                var dbAdd = new LoadInfoExtraAddrClassess();
                dbAdd.LoadInfoExtra = dblie;
                dbAdd.ViewValueDictionary = this._viewValueDictionary.Find(f => f.ViewValueDictionaryId == addr.ViewValueDictionaryId);
                this._db.Entry(dbAdd).State = EntityState.Added;
            }

            //truckBody - Added
            var truckBody = this._viewValueDictionary.Find(f => f.ViewValueDictionaryId == lieDTO.Required_truck_body.ViewValueDictionaryId);
            dblie.RequiredTruckBody = truckBody;
            //_db.Entry(truckBody).State = EntityState.Added;

            dblie.RequiredWaysOfLoading = new List<LoadInfoExtraWaysOfLoad>();
            foreach (var way in lieDTO.Required_ways_of_loading)
            {
                var dbWay = new LoadInfoExtraWaysOfLoad();
                dbWay.LoadInfoExtra = dblie;
                dbWay.ViewValueDictionary = this._viewValueDictionary.Find(f => f.ViewValueDictionaryId == way.ViewValueDictionaryId);
                this._db.Entry(dbWay).State = EntityState.Added;
            }

            dblie.TypeOfLoad = this._viewValueDictionary.Find(f => f.ViewValueDictionaryId == lieDTO.Type_of_load.ViewValueDictionaryId);
           

            //add reqTrack
            //foreach (var addr in lieDTO.Required_adr_classes)
            //{
            //    var dbAddr = this._viewValueDictionary.Find(f => f.ViewValueDictionaryID == addr.ViewValueDictionaryID);
            //    if (dbAddr != null)
            //    {
            //        dbAddr.RequiredAdrClasses = dblie;
            //        this._db.Entry(dbAddr).State = EntityState.Added;
            //    }
            //}

            //foreach (var way in lieDTO.Required_ways_of_loading)
            //{
            //    var dbWay = this._viewValueDictionary.Find(f => f.ViewValueDictionaryID == way.ViewValueDictionaryID);
            //    if (dbWay != null)
            //    {
            //        dbWay.RequiredWaysOfLoading = dblie;
            //        this._db.Entry(dbWay).State = EntityState.Added;
            //    }
            //}

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
        }

        public void LoadRoutePAlletMapper(LoadRoutePallet dbPallet, LoadRoutePalletDTO pallDTO)
        {
            dbPallet.Amount = pallDTO.Amount;
            dbPallet.Dimmension = pallDTO.Dimmension;
            dbPallet.Info = pallDTO.Info;
            dbPallet.IsExchangeable = pallDTO.Is_exchangeable;
            dbPallet.IsStackable = pallDTO.Is_stackable;
            dbPallet.IsEuroType = pallDTO.Type=="EURO" ? true: false;
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

#endregion

    }
}
