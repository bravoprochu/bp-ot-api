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

            //var dbRes = await this.LoadQueryable()
            //    .ToListAsync();

            //var res = new List<LoadDTO>();

            //foreach (var dbLoad in dbRes)
            //{
            //    res.Add(this.EtDTOLoad(dbLoad));
            //}

            //return Ok(res);

            var resList = new List<LoadDTO>();

            var resSell = await this.LoadQueryable()
                .Where(w => w.LoadBuy != null && w.InvoiceSell != null && w.LoadTransEu != null && w.LoadSell != null && w.InvoiceBuy!=null)
                .OrderByDescending(o => o.LoadId)
                .ToListAsync();

            var resTrans = await this.LoadTranEuQueryable()
                .Where(w => w.LoadBuy != null && w.InvoiceSell != null && w.LoadTransEu != null && w.LoadSell == null)
                .OrderByDescending(o => o.LoadId)
                .ToListAsync();

            var resBuy = await this.LoadBuyQueryable()
                .Where(w => w.LoadBuy != null && w.InvoiceSell!=null && w.LoadTransEu == null)
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

            var isSell= await this.LoadBuyQueryable()
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


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuy(int id, [FromBody] LoadDTO lDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbLoad = new Load();
            var dbLoadBuy = new LoadBuy();

            if (id == 0)
            {
                //invoice
                var dbInv = new InvoiceSell();
                await this.UpdateInvoiceSell(dbInv, lDTO);
                dbInv.Load = dbLoad;
                this._db.Entry(dbInv).State = EntityState.Added;

                //buy
                await this.UpdateLoadBuy(dbLoadBuy, lDTO.Buy);
                dbLoad.LoadNo = new bp.Pomocne.DocumentNumbers.DocNumber().GenNumberMonthYearNumber(await this._db.Load.Select(s => s.LoadNo).LastOrDefaultAsync(), lDTO.Buy.Buying_info.Date, '/').DocNumberCombined;
                this._db.Entry(dbLoadBuy).State = EntityState.Added;
                dbLoad.LoadBuy = dbLoadBuy;
                this._db.Entry(dbLoad).State = EntityState.Added;
            } else {
                dbLoad = await this.LoadBuyQueryable()
                    .FirstOrDefaultAsync(f => f.LoadId == id);

                if (dbLoad == null)
                {
                    return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono ładunku o Id: {id}"));
                }
                    //invoice
                var inv = new InvoiceSell();
                if (dbLoad.InvoiceSell != null) {
                    inv = await this._invoiceService.InvoiceSellQueryable()
                        .FirstOrDefaultAsync(f => f.InvoiceSellId == dbLoad.InvoiceSell.InvoiceSellId);
                }

                await this.UpdateInvoiceSell(inv, lDTO);
                if (dbLoad.InvoiceSell == null)
                {
                    inv.Load = dbLoad;
                    this._db.Entry(inv).State = EntityState.Added;
                }
                //buy
                await this.UpdateLoadBuy(dbLoad.LoadBuy, lDTO.Buy);

            }



            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {

                throw e;
            }
            return Ok(new { LoadId= dbLoad.LoadId});
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransEu(int id, [FromBody] LoadDTO lDTO)
        {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbLoad = await this.LoadBuyQueryable()
                .FirstOrDefaultAsync(f => f.LoadId == id);

            if (dbLoad == null) {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Błąd", $"Nie znaleziono ładunku o Id: {id}"));
            }

            //buy
            await this.UpdateLoadBuy(dbLoad.LoadBuy, lDTO.Buy);

            if (dbLoad.LoadTransEu == null)
            {
                var dbNewTransEu = new LoadTransEu();
                await this.UpdateLoadTransEu(dbNewTransEu, lDTO.TransEu);
                dbNewTransEu.Load = dbLoad;
                this._db.Entry(dbNewTransEu).State = EntityState.Added;
            }
            else {
                dbLoad = await this.LoadTranEuQueryable()
                        .FirstOrDefaultAsync(f => f.LoadId == id);
                await this.UpdateLoadTransEu(dbLoad.LoadTransEu, lDTO.TransEu);
            }

            await this._db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSell(int id, [FromBody] LoadDTO sDTO)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbLoad = new Load();
            var dbLoadSell = new LoadSell();
            if (id > 0) {
                dbLoad = await this.LoadTranEuQueryable()
                    .FirstOrDefaultAsync(f => f.LoadId == id);

                if (dbLoad == null) {
                    return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono ładunku o Id: {id}"));
                }

                await this.UpdateLoadBuy(dbLoad.LoadBuy, sDTO.Buy);
                await this.UpdateLoadTransEu(dbLoad.LoadTransEu, sDTO.TransEu);

                if (dbLoad.LoadSell == null)
                {
                    //invoiceBuy
                    var dbInv = new InvoiceBuy();
                    await this.UpdateInvoiceBuy(dbInv, sDTO);
                    dbInv.Load = dbLoad;
                    this._db.Entry(dbInv).State = EntityState.Added;


                    await this.UpdateLoadSell(dbLoadSell, sDTO.Sell);
                    dbLoadSell.Load = dbLoad;
                    this._db.Entry(dbLoadSell).State = EntityState.Added;
                }
                else {
                    dbLoad = await this.LoadQueryable()
                        .FirstOrDefaultAsync(f => f.LoadId == id);
                    dbLoadSell = dbLoad.LoadSell;
                    await this.UpdateLoadSell(dbLoadSell, sDTO.Sell);

                    var dbInv = dbLoad.InvoiceBuy ?? new InvoiceBuy();

                    await this.UpdateInvoiceBuy(dbInv, sDTO);
                    if (dbLoad.InvoiceBuy == null)
                    {
                        dbInv.Load = dbLoad;
                        this._db.Entry(dbInv).State = EntityState.Added;
                    }
                }


                try
                {
                    await this._db.SaveChangesAsync();
                }
                catch (Exception e)
                {

                    throw e;
                }
            }


            return NoContent();
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

            return NoContent();
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
                .Include(i => i.LoadBuy)
                .Include(i => i.LoadBuy).ThenInclude(i => i.BuyingInfo).ThenInclude(i => i.PaymentTerms).ThenInclude(i => i.PaymentTerm)
                .Include(i => i.LoadBuy).ThenInclude(i => i.BuyingInfo).ThenInclude(i => i.CurrencyNbp).ThenInclude(i => i.Currency)
                .Include(i => i.LoadBuy).ThenInclude(i => i.BuyingInfo).ThenInclude(i => i.Company).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadBuy).ThenInclude(i => i.LoadInfo)
                .Include(i => i.LoadBuy).ThenInclude(i => i.LoadInfo).ThenInclude(i => i.ExtraInfo)
                .Include(i => i.LoadBuy).ThenInclude(i => i.LoadInfo).ThenInclude(i => i.ExtraInfo).ThenInclude(i => i.RequiredAddrClassess).ThenInclude(i => i.ViewValueDictionary)
                .Include(i => i.LoadBuy).ThenInclude(i => i.LoadInfo).ThenInclude(i => i.ExtraInfo).ThenInclude(i => i.RequiredTruckBody)
                .Include(i => i.LoadBuy).ThenInclude(i => i.LoadInfo).ThenInclude(i => i.ExtraInfo).ThenInclude(i => i.RequiredWaysOfLoading).ThenInclude(i => i.ViewValueDictionary)
                .Include(i => i.LoadBuy).ThenInclude(i => i.LoadInfo).ThenInclude(i => i.ExtraInfo).ThenInclude(i => i.TypeOfLoad)
                .Include(i => i.LoadBuy).ThenInclude(i => i.Routes).ThenInclude(i => i.Address)
                .Include(i => i.LoadBuy).ThenInclude(i => i.Routes).ThenInclude(i => i.Pallets)
                .Include(i => i.LoadTransEu)
                .Include(i => i.InvoiceBuy)
                .Include(i => i.LoadSell)
                .Include(i => i.InvoiceSell).ThenInclude(i => i.ExtraInfo);
        }

        private IQueryable<Load> LoadTranEuQueryable()
        {
            return this.LoadBuyQueryable()
                .Include(i => i.LoadTransEu).ThenInclude(i=>i.ContactPersonsList).ThenInclude(i => i.CompanyEmployee)
                .Include(i => i.LoadTransEu).ThenInclude(i=>i.Price).ThenInclude(i => i.Currency)
                .Include(i => i.LoadTransEu).ThenInclude(i=>i.SellingCompany).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadTransEu).ThenInclude(i=>i.SellingCompany).ThenInclude(i => i.EmployeeList);
        }

        private IQueryable<Load> LoadQueryable()
        {
            return this.LoadTranEuQueryable()
                .Include(i => i.LoadSell).ThenInclude(i => i.ContactPersonsList).ThenInclude(i => i.CompanyEmployee)
                .Include(i => i.LoadSell).ThenInclude(i => i.Principal).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadSell).ThenInclude(i => i.Principal).ThenInclude(i => i.EmployeeList)
                .Include(i => i.LoadSell).ThenInclude(i => i.SellingInfo).ThenInclude(i => i.PaymentTerms).ThenInclude(i => i.PaymentTerm)
                .Include(i => i.LoadSell).ThenInclude(i => i.SellingInfo).ThenInclude(i => i.Company).ThenInclude(i => i.AddressList)
                .Include(i => i.LoadSell).ThenInclude(i => i.SellingInfo).ThenInclude(i => i.CurrencyNbp).ThenInclude(i => i.Currency)
                .Include(i => i.InvoiceBuy).ThenInclude(i => i.InvoicePosList)
                .Include(i => i.InvoiceBuy).ThenInclude(i => i.InvoiceTotal)
                .Include(i => i.InvoiceBuy).ThenInclude(i => i.PaymentTerms)
                .Include(i => i.InvoiceBuy).ThenInclude(i => i.RatesValuesList);
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
            lb.LoadBuyId = dbLoad.LoadBuy.LoadBuyId;
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
                ls.LoadSellId = dbLoad.LoadSell.LoadSellId;
                ls.ContactPersonsList = new List<CompanyEmployeeDTO>();
                foreach (var person in dbLoad.LoadSell.ContactPersonsList)
                {
                    ls.ContactPersonsList.Add(this._companyService.EtDTOEmployee(person.CompanyEmployee));
                }
                ls.Principal = this._companyService.EtDTOCompany(dbLoad.LoadSell.Principal);
                ls.Selling_info = this.EtDTOTradeInfo(dbLoad.LoadSell.SellingInfo);

                res.Sell = ls;
            }

            if (dbLoad.InvoiceSell != null)
            {
                res.InvoiceSellNo = dbLoad.InvoiceSell?.InvoiceNo;
                res.LoadExtraInfo = this._invoiceService.EtoDTOExtraInfo(dbLoad.InvoiceSell.ExtraInfo);
            }
            if (dbLoad.InvoiceBuy != null) {
                res.LoadExtraInfo.InvoiceBuyId = dbLoad.InvoiceBuy.InvoiceBuyId;
                res.LoadExtraInfo.InvoiceBuyNo = dbLoad.InvoiceBuy.InvoiceNo;
                res.LoadExtraInfo.InvoiceBuyRecived = dbLoad.InvoiceBuy.InvoiceRecived;
            }

            res.LoadId = dbLoad.LoadId;
            res.InvoiceSellNo = dbLoad.InvoiceSell?.InvoiceNo;
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
            res.LoadRouteId = route.LoadRouteId;
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
            res.LoadRoutePalletId = pallet.LoadRoutePalletId;
            res.Amount = pallet.Amount;
            res.Dimmension = pallet.Dimmension;
            res.Info = pallet.Info;
            res.Is_exchangeable = pallet.IsExchangeable;
            res.Is_stackable = pallet.IsStackable;
            res.Type = pallet.IsEuroType ? this.ViewValueDTOByName("EURO") : this.ViewValueDTOByName("Inne");
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
            var truckBodyDTO = lieDTO.Required_truck_body!=null ? this._viewValueDictionary.FirstOrDefault(f => f.ViewValueDictionaryId == lieDTO.Required_truck_body?.ViewValueDictionaryId) : null;
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
            if (lieDTO.Required_adr_classes != null)
            {
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

            if (lieDTO.Required_ways_of_loading != null)
            {
                foreach (var wayDTO in lieDTO.Required_ways_of_loading)
                {
                    var wayOfLoadDTO = this._viewValueDictionary.FirstOrDefault(f => f.ViewValueDictionaryId == wayDTO.ViewValueDictionaryId);
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
            }

            dbLie.TypeOfLoad = this._viewValueDictionary.FirstOrDefault(f => f.ViewValueDictionaryId == lieDTO.Type_of_load?.ViewValueDictionaryId);
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
                    this._db.Entry(dbPal).State = EntityState.Deleted;
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
            bool isEuro= pallDTO.Type.Value == "euro" ? true : false;
            dbPallet.Amount = pallDTO.Amount;
            dbPallet.Dimmension = isEuro? null: pallDTO.Dimmension;
            dbPallet.Info = pallDTO.Info;
            dbPallet.IsExchangeable = isEuro ? null : pallDTO.Is_exchangeable;
            dbPallet.IsStackable = isEuro ? null : pallDTO.Is_stackable;
            dbPallet.IsEuroType = isEuro;
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
            if (dbTi.PaymentTerms == null)
            {
                paymentTerms.TradeInfo = dbTi;
                this._db.Entry(paymentTerms).State = EntityState.Added;
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
                var find = lDTO.Routes.FirstOrDefault(w => w.LoadRouteId == dbRoute.LoadRouteId);
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

        private async Task UpdateLoadTransEu(LoadTransEu dbTrans, LoadTransEuDTO tDTO)
        {
            //contactPersonsList
            //remove from db deleted
            if (dbTrans.ContactPersonsList != null)
            {
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
            dbTrans.TransEuId = tDTO.TransEuId;
        }

        private async Task UpdateLoadSell(LoadSell dbSell, LoadSellDTO sDTO)
        {
            if (dbSell.ContactPersonsList != null)
            {
                if (dbSell.ContactPersonsList?.Count > 0)
                {
                    foreach (var contact in dbSell.ContactPersonsList)
                    {
                        var found = sDTO.ContactPersonsList.Where(w => w.CompanyEmployeeId == contact.CompanyEmployeeId).FirstOrDefault();
                        if (found == null)
                        {
                            this._db.Entry(contact).State = EntityState.Deleted;
                        }
                    }
                }
            }
            //modify or add new..
            foreach (var contactDTO in sDTO.ContactPersonsList)
            {
                var foundDb = dbSell.ContactPersonsList?.FirstOrDefault(f => f.CompanyEmployeeId == contactDTO.CompanyEmployeeId);
                if (foundDb == null)
                {
                    //add new
                    var newContactDb = new LoadSellContactPersons
                    {
                        CompanyEmployee = await this._db.CompanyEmployee.FindAsync(contactDTO.CompanyEmployeeId),
                        LoadSell = dbSell
                    };
                    this._db.Entry(newContactDb).State = EntityState.Added;
                }
                else
                {
                    //modify when companyID is different
                    if (foundDb.CompanyEmployeeId != contactDTO.CompanyEmployeeId)
                    {
                        foundDb.CompanyEmployee = await this._db.CompanyEmployee.FindAsync(contactDTO.CompanyEmployeeId);
                    }
                }
            }

            //principal
            if (dbSell.Principal == null || dbSell.Principal?.CompanyId != sDTO.Principal.CompanyId)
            {
                dbSell.Principal = await this._db.Company.FindAsync(sDTO.Principal.CompanyId);
            }
            //sellingInfo
            var tradeInfo = dbSell.SellingInfo ?? new TradeInfo();
            await this.TradeInfoMapper(tradeInfo, sDTO.Selling_info);
            if (dbSell.SellingInfo == null)
            {
                tradeInfo.LoadSell = dbSell;
                this._db.Entry(tradeInfo).State = EntityState.Added;
            }

        }

        private async Task UpdateInvoiceBuy(InvoiceBuy dbInv, LoadDTO lDTO)
        {
            
            var tradeInfoDTO = lDTO.Sell.Selling_info;

            var dbCurr = dbInv.Currency ?? new Currency();
            if (dbInv.Currency == null || dbInv.Currency.CurrencyId != tradeInfoDTO.Price.Currency.CurrencyId)
            {
                dbInv.Currency = this._invoiceService._currencyList.FirstOrDefault(f => f.CurrencyId == tradeInfoDTO.Price.Currency.CurrencyId);
            }

            dbInv.DateOfIssue = tradeInfoDTO.Date;
            dbInv.InvoiceNo = dbInv.InvoiceNo ?? $"Pro forma {lDTO.LoadNo}";

            //invoice pos
            var price = tradeInfoDTO.Price;
            var brutto = Math.Round(price.Price * 1.23, 2);

            var dbPos = new InvoicePos();
            var posDTO = new InvoicePosDTO
            {
                Brutto_value = brutto,
                Measurement_unit = "szt",
                Name = $"Usługa transportowa",
                Netto_value = price.Price,
                Quantity = 1,
                Unit_price = price.Price,
                Vat_rate = "23",
                Vat_unit_value = brutto - price.Price,
                Vat_value = brutto - price.Price
            };
            this._invoiceService.InvoicePosMapperFromDTO(dbPos, posDTO);
            if (dbInv.InvoicePosList == null || dbInv.InvoicePosList.Count == 0)
            {
                dbPos.InvoiceBuy = dbInv;
                this._db.Entry(dbPos).State = EntityState.Added;
            }
            else {
                dbPos = dbInv.InvoicePosList.FirstOrDefault();
                this._invoiceService.InvoicePosMapperFromDTO(dbPos, posDTO);
            }

            var dbTotal = dbInv.InvoiceTotal ?? new InvoiceTotal();
            dbTotal.TotalBrutto = brutto;
            dbTotal.TotalNetto = price.Price;
            dbTotal.TotalTax = brutto - price.Price;
            if (dbInv.InvoiceTotal == null) {
                dbTotal.InvoiceBuy = dbInv;
                this._db.Entry(dbTotal).State = EntityState.Added;
            }

            var dbPaymentTerms = dbInv.PaymentTerms ?? new PaymentTerms();
            this._invoiceService.PaymentTermsMapper(dbPaymentTerms, tradeInfoDTO.Payment_terms);
            if (dbInv.PaymentTerms == null) {
                dbPaymentTerms.InvoiceBuy = dbInv;
                this._db.Entry(dbPaymentTerms).State = EntityState.Added;
            }


            if (dbInv.RatesValuesList == null || dbInv.RatesValuesList.Count == 0)
            {
                var dbRate = new RateValue();
                dbRate.BruttoValue = brutto;
                dbRate.NettoValue = price.Price;
                dbRate.VatRate = "23";
                dbRate.VatValue = brutto - price.Price;

                dbRate.InvoiceBuy = dbInv;
                this._db.Entry(dbRate).State = EntityState.Added;

            }
            else {
                var dbRate = dbInv.RatesValuesList.FirstOrDefault();
                dbRate.BruttoValue = brutto;
                dbRate.NettoValue = price.Price;
                dbRate.VatRate = "23";
                dbRate.VatValue = brutto - price.Price;
            }


            if (dbInv.Seller == null || dbInv.Seller.CompanyId != tradeInfoDTO.Company.CompanyId)
            {
                dbInv.Seller = await this._db.Company.FirstOrDefaultAsync(f => f.CompanyId == tradeInfoDTO.Company.CompanyId);
            }
            dbInv.SellingDate = tradeInfoDTO.Date;





            
        }

        private async Task UpdateInvoiceSell(InvoiceSell dbInv, LoadDTO lDTO)
        {
            var sInfoDTO = lDTO.Buy.Buying_info;
            if (dbInv.Buyer == null || dbInv.BuyerId != sInfoDTO.Company.CompanyId)
            {
                dbInv.Buyer = await this._db.Company.FirstOrDefaultAsync(f => f.CompanyId == sInfoDTO.Company.CompanyId);
            }
            dbInv.Currency = this._invoiceService._currencyList.FirstOrDefault(f => f.CurrencyId == sInfoDTO.Price.Currency.CurrencyId);
            dbInv.DateOfIssue = sInfoDTO.Date;
            var extraInfo = dbInv.ExtraInfo ?? new InvoiceExtraInfo();
            extraInfo.LoadNo = lDTO.LoadNo;
            if (dbInv.ExtraInfo == null) {
                extraInfo.InvoiceSell = dbInv;
                this._db.Entry(extraInfo).State = EntityState.Added;
            }

            dbInv.InvoiceNo=dbInv.InvoiceNo ?? new bp.Pomocne.DocumentNumbers.DocNumber().GenNumberMonthYearNumber(this._db.InvoiceSell.LastOrDefault().InvoiceNo, sInfoDTO.Date, '/').DocNumberCombined;

            var dbPaymentTerms = dbInv.PaymentTerms ?? new PaymentTerms();
            this._invoiceService.PaymentTermsMapper(dbPaymentTerms, sInfoDTO.Payment_terms);
            if (dbInv.PaymentTerms == null) {
                dbPaymentTerms.InvoiceSell = dbInv;
                this._db.Entry(dbPaymentTerms).State = EntityState.Added;
            }
            
            var invPosDTO = new InvoicePosDTO();
            invPosDTO.Brutto_value = sInfoDTO.Price.Price;
            invPosDTO.Measurement_unit = "szt.";
            invPosDTO.Name = "Usługa transportowa";
            invPosDTO.Netto_value = sInfoDTO.Price.Price;
            invPosDTO.Quantity = 1;
            invPosDTO.Unit_price= sInfoDTO.Price.Price;
            invPosDTO.Vat_rate = "-";
            invPosDTO.Vat_unit_value = 0;
            invPosDTO.Vat_value = 0;
            if (dbInv.InvoicePosList == null)
            {
                var dbInvPos = new InvoicePos();
                this._invoiceService.InvoicePosMapperFromDTO(dbInvPos, invPosDTO);
                dbInvPos.InvoiceSell = dbInv;
                this._db.Entry(dbInvPos).State = EntityState.Added;
            }
            else
            {
                var dbInvPos = dbInv.InvoicePosList.FirstOrDefault();
                this._invoiceService.InvoicePosMapperFromDTO(dbInvPos, invPosDTO);
            }

            if (dbInv.Seller == null)
            {
                dbInv.Seller = await this._db.Company.FirstOrDefaultAsync(f => f.CompanyId == 1);
            }

            dbInv.SellingDate = sInfoDTO.Date;

            var ratePosDTO = new InvoiceRatesValuesDTO();
            ratePosDTO.Brutto_value = sInfoDTO.Price.Price;
            ratePosDTO.Netto_value = sInfoDTO.Price.Price;
            ratePosDTO.Vat_rate = "-";

            if (dbInv.RatesValuesList == null)
            {
                var dbNewRate = new RateValue();
                this._invoiceService.InvoiceRateMapper(dbNewRate, ratePosDTO);
                dbNewRate.InvoiceSell = dbInv;
                this._db.Entry(dbNewRate).State = EntityState.Added;
            }
            else {
                var dbRate = dbInv.RatesValuesList.FirstOrDefault();
                this._invoiceService.InvoiceRateMapper(dbRate, ratePosDTO);
            }

            var dbInvTotal = dbInv.InvoiceTotal ?? new InvoiceTotal();
            dbInvTotal.TotalBrutto = sInfoDTO.Price.Price;
            dbInvTotal.TotalNetto = sInfoDTO.Price.Price;
            dbInvTotal.TotalTax = 0;
            if (dbInv.InvoiceTotal == null)
            {
                dbInvTotal.InvoiceSell = dbInv;
                this._db.Entry(dbInvTotal).State = EntityState.Added;
            }
        }

        

        private ValueViewValueDTO ViewValueDTOById(int id)
        {
            var v = this._viewValueDictionary.FirstOrDefault(f => f.ViewValueDictionaryId == id);
            return v != null ? this.ValueViewValueMapper(v) : null;
        }

        private ValueViewValueDTO ViewValueDTOByName(string name) {
            var v = this._viewValueDictionary.FirstOrDefault(f => f.ViewValue == name);
            return v != null ? this.ValueViewValueMapper(v) : null;
        }

        private ValueViewValueDTO ValueViewValueMapper(ViewValueDictionary dict)
        {
            return new ValueViewValueDTO
            {
                Value = dict.Value,
                ViewValue = dict.ViewValue,
                ViewValueDictionaryId = dict.ViewValueDictionaryId,
                ViewValueGroupNameId = dict.ViewValueGroupNameId
            };
        }

        
            

#endregion

    }
}
