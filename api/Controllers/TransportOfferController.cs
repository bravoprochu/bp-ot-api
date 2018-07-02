using bp.ot.s.API.Entities.Context;
using bp.ot.s.API.Entities.Dane.Company;
using bp.ot.s.API.Entities.Dane.Invoice;
using bp.ot.s.API.Entities.Dane.Transport;
using bp.ot.s.API.Entities.Dane.TransportOffer;
using bp.ot.s.API.Models.Load;
using bp.shared;
using bp.shared.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace bp.ot.s.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Spedytor")]
    public class TransportOfferController:Controller
    {
        private readonly OfferTransDbContextDane _db;
        private readonly InvoiceService _invoiceService;
        private readonly CompanyService _companyService;
        private readonly CommonFunctions _commonFunctions;

        public TransportOfferController(OfferTransDbContextDane db, InvoiceService invoiceService, CompanyService companyService, CommonFunctions commonFunctions)
        {
            this._db = db;
            this._invoiceService = invoiceService;
            this._companyService = companyService;
            this._commonFunctions = commonFunctions;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dbRes = await TransportOfferQueryable()
                .FirstOrDefaultAsync(f => f.TransportOfferId == id);

            if (dbRes == null) {
                //return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono transportu o Id: {id} "));
                return NotFound();
            }


            if (dbRes.InvoiceSell != null) {
                return Ok(new { info = $"Aby usunąć ładunek {dbRes.OfferNo} należy najpierw usunąć fakturę sprzedaży: {dbRes.InvoiceSell.InvoiceNo}" });
            }



            this._db.Entry(dbRes).State = EntityState.Deleted;

            await this._db.SaveChangesAsync();


            return NoContent();
        }

        [HttpGet("{dateStart}/{dateEnd}")]
        public async Task<IActionResult> GetAll(DateTime dateStart, DateTime dateEnd)
        {
            dateEnd = bp.shared.DateHelp.DateHelpful.DateRangeDateTo(dateEnd);

            var dbRes = await this.TransportOfferQueryable()
                .Where(w=>w.Date>=dateStart && w.Date<=dateEnd)
                .OrderByDescending(o=>o.TransportOfferId)
                .ToListAsync();

            var res = new List<TransportOfferListDTO>();
            foreach (var dBTransp in dbRes)
            {
                res.Add(this.TransportDTOtoList(this.EtDTOTransportOffer(dBTransp)));
            }
            return Ok(res);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dbTrans = new TransportOffer();
            if (id == 0)
            {
                dbTrans = await this.TransportOfferQueryable()
                    .OrderByDescending(o => o.TransportOfferId)
                    .FirstOrDefaultAsync();
            }
            else {
                dbTrans = await this.TransportOfferQueryable()
                    .FirstOrDefaultAsync(f => f.TransportOfferId == id);
            }

            if (dbTrans == null) {
                //return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Błąd", $"Nie znaleziono Transportu o Id: {id}"));
                return NotFound();
            }

            return Ok(this.EtDTOTransportOffer(dbTrans));
        }

        [HttpGet("{id}/{invoiceInPLN}")]
        public async Task<IActionResult> InvoiceSellGen(int id, bool invoiceInPLN)
        {
            var dbRes = await this.TransportOfferQueryable()
                .FirstOrDefaultAsync(f => f.TransportOfferId == id);

            if (dbRes == null)
            {
                //return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono transportu o Id: {id}"));
                return NotFound();
            }

            if (dbRes.InvoiceSell != null)
            {
                return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Transportu o Id: {id}, ma już utworzoną FV {dbRes.InvoiceSell.InvoiceNo}"));
            }

            
            var dbInv = new InvoiceSell();
            var transportDTO = this.EtDTOTransportOffer(dbRes);
            transportDTO.InvoiceInPLN = invoiceInPLN;
            await this.UpdateInvoiceSell(dbInv, transportDTO);
            dbInv.TransportOffer = dbRes;
            this._db.Entry(dbInv).State = EntityState.Added;

            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception e)
            {

                throw e;
            }
            
            

            return NoContent();
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TransportOfferDTO tDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbTrans = new TransportOffer();
            if (id == 0)
            {
                await this.TransportOfferMapper(dbTrans, tDTO);
                this._db.Entry(dbTrans).State = EntityState.Added;
            }
            else {
                dbTrans = await this.TransportOfferQueryable()
                    .FirstOrDefaultAsync(f => f.TransportOfferId == id);

                if (dbTrans == null){
                    return BadRequest(bp.PomocneLocal.ModelStateHelpful.ModelStateHelpful.ModelError("Error", $"Nie znaleziono transportu o Id: {id}"));
                }

                await this.TransportOfferMapper(dbTrans, tDTO);
            }

            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception e)
            {

                throw e;
            }
            


            return NoContent();
        }


        private IQueryable<TransportOffer> TransportOfferQueryable()
        {
            return this._db.TransportOffer
                .Include(i => i.Company).ThenInclude(i => i.AddressList)
                .Include(i => i.Company).ThenInclude(i => i.EmployeeList)
                .Include(i=>i.CurrencyNbp).ThenInclude(i=>i.Currency)
                .Include(i=>i.InvoiceSell)
                .Include(i => i.Load)
                .Include(i => i.PaymentTerms).ThenInclude(i => i.PaymentTerm)
                .Include(i => i.Unload);
        }


        private TransportOfferDTO EtDTOTransportOffer(TransportOffer dbTrans)
        {
            var res = new TransportOfferDTO();
            res.CreationInfo =new bp.shared.CommonFunctions().EtDTOCreationInfoMapper((CreationInfo)dbTrans);
            res.Info = dbTrans.Info;
            res.InvoiceSellId = dbTrans.InvoiceSell!=null ? dbTrans.InvoiceSellId : null;
            res.InvoiceSellNo = dbTrans.InvoiceSell?.InvoiceNo;
            res.Load = this.EtDTOTransportOfferAddress(dbTrans.Load);
            res.OfferNo = dbTrans.OfferNo;
            res.TradeInfo = new TradeInfoDTO();


            res.TradeInfo.Company = this._companyService.EtDTOCompany(dbTrans.Company);
            res.TradeInfo.Date = dbTrans.Date;
            res.TradeInfo.PaymentTerms = this._invoiceService.EtoDTOPaymentTerms(dbTrans.PaymentTerms);
            res.TradeInfo.Price = this._invoiceService.EtoDTOCurrencyNbp(dbTrans.CurrencyNbp);          

            res.TransportOfferId = dbTrans.TransportOfferId;
            res.Unload = this.EtDTOTransportOfferAddress(dbTrans.Unload);
            
            return res;
        }
        private TransportOfferAddressDTO EtDTOTransportOfferAddress(TransportOfferAddress dbTransAddress)
        {
            var res = new TransportOfferAddressDTO();
            res.Date = dbTransAddress.Date;
            res.Locality = dbTransAddress.Locality;
            res.PostalCode = dbTransAddress.PostalCode;
            res.TransportOfferAddressId = dbTransAddress.TransportOfferAddressId;
            return res;
        }

        private async Task TransportOfferMapper(TransportOffer dbTrans, TransportOfferDTO tDTO)
        {
            //if(dbTrans.Company==null || dbTrans.CompanyId!=tDTO.TradeInfo.Company.CompanyId)
            //{
            //    dbTrans.Company =await this._db.Company.FirstOrDefaultAsync(f => f.CompanyId == tDTO.TradeInfo.Company.CompanyId);

            //}

            //if (dbTrans.Company == null) {
            //    dbTrans.Company = new Company();
            //    _db.Entry(dbTrans.Company).State = EntityState.Added;
            //}
            dbTrans.Company= await this._companyService.CompanyMapper(dbTrans.Company, tDTO.TradeInfo.Company);

            if (dbTrans.CurrencyNbp == null) {
                dbTrans.CurrencyNbp = new CurrencyNbp();
                _db.Entry(dbTrans.CurrencyNbp).State = EntityState.Added;
            }
            this._invoiceService.MapperCurrencyNbp(dbTrans.CurrencyNbp, tDTO.TradeInfo.Price);

            dbTrans.Date = tDTO.TradeInfo.Date;
            dbTrans.Info = tDTO.Info;
            var dbLoadAdd = dbTrans.Load ?? new TransportOfferAddress();

            //load
            if (dbTrans.Load == null) {
                dbTrans.Load = new TransportOfferAddress();
                _db.Entry(dbTrans.Load).State = EntityState.Added;
            }
            this.TransportOfferAddressMapper(dbTrans.Load, tDTO.Load);
            

            dbTrans.OfferNo = tDTO.OfferNo;

            var dbPaymentTerms = dbTrans.PaymentTerms ?? new PaymentTerms();
            this._invoiceService.MapperPaymentTerms(dbPaymentTerms, tDTO.TradeInfo.PaymentTerms);
            if (dbTrans.PaymentTerms == null) {
                dbPaymentTerms.TransportOffer = dbTrans;
                this._db.Entry(dbPaymentTerms).State = EntityState.Added;
            }

            //unload
            if (dbTrans.Unload == null)
            {
                dbTrans.Unload = new TransportOfferAddress();
                _db.Entry(dbTrans.Unload).State = EntityState.Added;
            }
            this.TransportOfferAddressMapper(dbTrans.Unload, tDTO.Unload);

            this._commonFunctions.CreationInfoUpdate((CreationInfo)dbTrans, tDTO.CreationInfo, User);
          

        }

        private TransportOfferListDTO TransportDTOtoList(TransportOfferDTO dto)
        {
            var res = new TransportOfferListDTO();

            res.Currency = dto.TradeInfo.Price.Currency.Name;
            res.DocumentNo = dto.OfferNo;
            res.Fracht = dto.TradeInfo.Price.Price;
            res.LoadDate = bp.shared.DateHelp.DateHelpful.FormatDateToYYYYMMDD(dto.Load.Date);
            res.LoadPlace = dto.Load.Locality;
            res.LoadPostalCode = dto.Load.PostalCode;
            res.Seller = dto.TradeInfo.Company.Short_name;

            if (dto.InvoiceSellId.HasValue)
            {
                res.StatusCode = "FS";
            }

            res.TransportOfferId = dto.TransportOfferId.Value;
            res.UnloadDate = bp.shared.DateHelp.DateHelpful.FormatDateToYYYYMMDD(dto.Unload.Date);
            res.UnloadPlace = dto.Unload.Locality;
            res.UnloadPostalCode = dto.Unload.PostalCode;
            

            return res;
        }

        private void TransportOfferAddressMapper(TransportOfferAddress dbTransAddress, TransportOfferAddressDTO addDTO)
        {
            dbTransAddress.Date = addDTO.Date;
            dbTransAddress.Locality = addDTO.Locality;
            dbTransAddress.PostalCode = addDTO.PostalCode;
        }

        private async Task UpdateInvoiceSell(InvoiceSell dbInv, TransportOfferDTO dto)
        {
            var tradeInfoDTO = dto.TradeInfo;
            if (dbInv.Buyer == null || dbInv.BuyerId != tradeInfoDTO.Company.CompanyId)
            {
                //dbInv.Buyer = await this._db.Company.FirstOrDefaultAsync(f => f.CompanyId == tradeInfoDTO.Company.CompanyId);
                dbInv.Buyer = await this._companyService.CompanyMapper(dbInv.Buyer, dto.TradeInfo.Company);
            }
            if (dto.InvoiceInPLN)
            {
                dbInv.Currency = this._invoiceService._currencyList.FirstOrDefault(f => f.Name=="PLN");
            }
            else {
                dbInv.Currency = this._invoiceService._currencyList.FirstOrDefault(f => f.CurrencyId == tradeInfoDTO.Price.Currency.CurrencyId);
            }
            
            dbInv.DateOfIssue = DateTime.Now;
            var extraInfo = dbInv.ExtraInfo ?? new InvoiceExtraInfo();
            extraInfo.LoadNo = dto.OfferNo;
            if (dbInv.ExtraInfo == null)
            {
                extraInfo.InvoiceSell = dbInv;
                this._db.Entry(extraInfo).State = EntityState.Added;
            }

            dbInv.InvoiceNo = await this._invoiceService.GetNextInvoiceNo(dto.TradeInfo.Date);

            //invoice pos
            var price = dto.InvoiceInPLN ? tradeInfoDTO.Price.PlnValue: tradeInfoDTO.Price.Price;
            var brutto = Math.Round(price * 1.23, 2);

            var dbPos = new InvoicePos();
            var posDTO = new InvoiceLineDTO
            {
                Brutto_value = brutto,
                Measurement_unit = "szt",
                Name = $"Usługa transportowa",
                Netto_value = price,
                Quantity = 1,
                Unit_price = price,
                Vat_rate = "23",
                Vat_unit_value = brutto - price,
                Vat_value = brutto - price
            };
            this._invoiceService.MapperLine(dbPos, posDTO);
            if (dbInv.InvoicePosList == null || dbInv.InvoicePosList.Count == 0)
            {
                dbPos.InvoiceSell = dbInv;
                this._db.Entry(dbPos).State = EntityState.Added;
            }
            else
            {
                dbPos = dbInv.InvoicePosList.FirstOrDefault();
                this._invoiceService.MapperLine(dbPos, posDTO);
            }

            var dbTotal = dbInv.InvoiceTotal ?? new InvoiceTotal();
            dbTotal.TotalBrutto = brutto;
            dbTotal.TotalNetto = price;
            dbTotal.TotalTax = brutto - price;
            if (dbInv.InvoiceTotal == null)
            {
                dbTotal.InvoiceSell = dbInv;
                this._db.Entry(dbTotal).State = EntityState.Added;
            }

            var dbPaymentTerms = dbInv.PaymentTerms ?? new PaymentTerms();
            this._invoiceService.MapperPaymentTerms(dbPaymentTerms, tradeInfoDTO.PaymentTerms);
            if (dbInv.PaymentTerms == null)
            {
                dbPaymentTerms.InvoiceSell = dbInv;
                this._db.Entry(dbPaymentTerms).State = EntityState.Added;
            }


            if (dbInv.RatesValuesList == null || dbInv.RatesValuesList.Count == 0)
            {
                var dbRate = new RateValue();
                dbRate.BruttoValue = brutto;
                dbRate.NettoValue = price;
                dbRate.VatRate = "23";
                dbRate.VatValue = brutto - price;

                dbRate.InvoiceSell = dbInv;
                this._db.Entry(dbRate).State = EntityState.Added;

            }
            else
            {
                var dbRate = dbInv.RatesValuesList.FirstOrDefault();
                dbRate.BruttoValue = brutto;
                dbRate.NettoValue = price;
                dbRate.VatRate = "23";
                dbRate.VatValue = brutto - price;
            }

            if (dbInv.Seller == null) {
                dbInv.Seller = await this._companyService.Owner();
            }
            dbInv.SellingDate = dto.TradeInfo.Date;

            var dbInvTotal = dbInv.InvoiceTotal ?? new InvoiceTotal();
            dbInvTotal.TotalBrutto = brutto;
            dbInvTotal.TotalNetto = tradeInfoDTO.Price.Price;
            dbInvTotal.TotalTax = brutto-tradeInfoDTO.Price.Price;
            if (dbInv.InvoiceTotal == null)
            {
                dbInvTotal.InvoiceSell = dbInv;
                this._db.Entry(dbInvTotal).State = EntityState.Added;
            }
        }
    }


}
