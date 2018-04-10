using bp.ot.s.API.Entities.Dane.Invoice;
using bp.ot.s.API.Models.Load;
using bp.shared.IdentityHelp.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace bp.ot.s.API.Entities.Context
{

    public class OfferTransDbContextInitialDataIdent
    {
        private readonly OfferTransDbContextDane _db;

        public OfferTransDbContextInitialDataIdent(OfferTransDbContextDane daneContext)
        {
            _db = daneContext;
        }

        public async Task Initialize()
        {
            await _db.Database.MigrateAsync();
            

            var roles = _db.Roles.ToList();

            await CreateRole(roles, IdentConst.Administrator);
            await CreateRole(roles, IdentConst.Manager);
            await CreateRole(roles, IdentConst.Spedytor);
            await CreateRole(roles, IdentConst.Finanse);
            await this.ViewValueGroupNameInit();
            if (_db.Currency.Where(w => w.CurrencyId == 5).FirstOrDefault() == null)
            {
                this.CurrencyInit();

                await _db.SaveChangesAsync();
            }
            if (_db.PaymentTerm.Where(w => w.PaymentTermId == 2).FirstOrDefault() == null) {
                this.PaymentTermInit();
                await _db.SaveChangesAsync();
            }

            AssignRoles(_db.Roles.Where(w => w.Name == IdentConst.Administrator).FirstOrDefault());
            await _db.SaveChangesAsync();

        }

        private async Task CreateRole(List<IdentityRole> roleBaseList, string roleName)
        {
            if (roleBaseList.Find(f => f.Name == roleName) == null)
            {
                await _db.Roles.AddAsync(new IdentityRole(roleName));
            }
        }

        private void AssignRoles(IdentityRole adminRole)
        {
            var bravoprochu = _db.Users.Where(w => w.UserName == "bravoprochu@gmail.com").Select(s => s).FirstOrDefault();
            if (bravoprochu != null) {return; }
            if (_db.UserRoles.Where(w => w.UserId == bravoprochu.Id && w.RoleId==adminRole.Id).FirstOrDefault() != null) { return; }
            _db.UserRoles.Add(new IdentityUserRole<string>()
            {
                RoleId = adminRole.Id,
                UserId = bravoprochu.Id
            });
        }

        private void CurrencyInit()
        {
            List<Currency> currList = new List<Currency>();

            currList.Add(new Currency {Name = "THB", Description = "bat (Tajlandia)" });
            currList.Add(new Currency {Name = "USD", Description = "dolar amerykański" });
            currList.Add(new Currency {Name = "AUD", Description = "dolar australijski" });
            currList.Add(new Currency {Name = "HKD", Description = "dolar Hongkongu" });
            currList.Add(new Currency {Name = "CAD", Description = "dolar kanadyjski" });
            currList.Add(new Currency {Name = "NZD", Description = "dolar nowozelandzki" });
            currList.Add(new Currency {Name = "SGD", Description = "dolar singapurski" });
            currList.Add(new Currency {Name = "EUR", Description = "euro" });
            currList.Add(new Currency {Name = "HUF", Description = "forint (Węgry)" });
            currList.Add(new Currency {Name = "CHF", Description = "frank szwajcarski" });
            currList.Add(new Currency {Name = "GBP", Description = "funt szterling" });
            currList.Add(new Currency {Name = "UAH", Description = "hrywna (Ukraina)" });
            currList.Add(new Currency {Name = "JPY", Description = "jen (Japonia)" });
            currList.Add(new Currency {Name = "CZK", Description = "korona czeska" });
            currList.Add(new Currency {Name = "DKK", Description = "korona duńska" });
            currList.Add(new Currency {Name = "ISK", Description = "korona islandzka" });
            currList.Add(new Currency {Name = "NOK", Description = "korona norweska" });
            currList.Add(new Currency {Name = "SEK", Description = "korona szwedzka" });
            currList.Add(new Currency {Name = "HRK", Description = "kuna (Chorwacja)" });
            currList.Add(new Currency {Name = "RON", Description = "lej rumuński" });
            currList.Add(new Currency {Name = "BGN", Description = "lew (Bułgaria)" });
            currList.Add(new Currency {Name = "TRY", Description = "lira turecka" });
            currList.Add(new Currency {Name = "ILS", Description = "nowy izraelski szekel" });
            currList.Add(new Currency {Name = "CLP", Description = "peso chilijskie" });
            currList.Add(new Currency {Name = "PHP", Description = "peso filipińskie" });
            currList.Add(new Currency {Name = "MXN", Description = "peso meksykańskie" });
            currList.Add(new Currency {Name = "ZAR", Description = "rand (Republika Południowej Afryki)" });
            currList.Add(new Currency {Name = "BRL", Description = "real (Brazylia)" });
            currList.Add(new Currency {Name = "MYR", Description = "ringgit (Malezja)" });
            currList.Add(new Currency {Name = "RUB", Description = "rubel rosyjski" });
            currList.Add(new Currency {Name = "IDR", Description = "rupia indonezyjska" });
            currList.Add(new Currency {Name = "INR", Description = "rupia indyjska" });
            currList.Add(new Currency {Name = "KRW", Description = "won południowokoreański" });
            currList.Add(new Currency {Name = "CNY", Description = "yuan renminbi (Chiny)" });
            currList.Add(new Currency {Name = "XDR", Description = "SDR (MFW)" });
            currList.Add(new Currency {Name = "PLN", Description = "polski złoty" });



            foreach (var curr in currList)
            {
                this._db.Entry(curr).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }

        }

        private void PaymentTermInit()
        {

            var termList = new List<PaymentTerm>();
            termList.Add(new PaymentTerm { IsDescription = false, IsPaymentDate = false, Name = "gotówka" });
            termList.Add(new PaymentTerm { IsDescription = true, IsPaymentDate = true, Name = "gotówka w terminie" });
            termList.Add(new PaymentTerm { IsDescription = false, IsPaymentDate = true, Name = "przelew" });
            termList.Add(new PaymentTerm { IsDescription = false, IsPaymentDate = false, Name = "karta płatnicza" });

            foreach (var term in termList)
            {
                this._db.Entry(term).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }
        }

        private async Task ViewValueGroupNameInit()
        {

            List<ViewValueGroupName> GroupNamesList = new List<ViewValueGroupName>();
            var dbGroupNames = this._db.ViewValueGroupName.ToList();

            GroupNamesList.Add(new ViewValueGroupName { Description = "waysOfLoad", Name = "waysOfLoad" });
            GroupNamesList.Add(new ViewValueGroupName { Description = "addrClasses", Name = "addrClasses" });
            GroupNamesList.Add(new ViewValueGroupName { Description = "truckBody", Name = "truckBody" });
            GroupNamesList.Add(new ViewValueGroupName { Description = "typeOfLoad", Name = "typeOfLoad" });
            GroupNamesList.Add(new ViewValueGroupName { Description = "loadRoutePalletType", Name = "loadRoutePalletType" });


            foreach (var group in GroupNamesList)
            {
                if (dbGroupNames.Where(w => w.Name == group.Name).FirstOrDefault() == null)
                {
                    this._db.Entry(group).State = EntityState.Added;
                }
            }

            await this._db.SaveChangesAsync();

            //viewValue
            List<ViewValueDictionary> ViewValueList = new List<ViewValueDictionary>();
            dbGroupNames = this._db.ViewValueGroupName.ToList();
            var dbViewList = this._db.ViewValueDictionary.ToList();


            ViewValueList.Add(new ViewValueDictionary { Value = "1", ViewValue = "Materiały i przedmioty wybuchowe", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "2", ViewValue = "Gazy", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "3", ViewValue = "Materiały ciekłe zapalne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "4.1", ViewValue = "Materiały stałe zapalne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "4.2", ViewValue = "Materiały samozapalne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "4.3", ViewValue = "Materiały wytwarzające w zetknięciu z wodą gazy palne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "5.1", ViewValue = "Materiały utleniające", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "5.2", ViewValue = "Nadtlenki organiczne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "6.1", ViewValue = "Materiały trujące", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "6.2", ViewValue = "Materiały zakaźne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "7", ViewValue = "Materiały promieniotwórcze", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "8", ViewValue = "Materiały żrące", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "9", ViewValue = "Różne materiały i przedmioty niebezpieczne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "addrClasses").FirstOrDefault() });

            ViewValueList.Add(new ViewValueDictionary { Value = "top", ViewValue = "góra", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "waysOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "back", ViewValue = "tyłem", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "waysOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "side", ViewValue = "bokiem", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "waysOfLoad").FirstOrDefault() });

            ViewValueList.Add(new ViewValueDictionary { Value = "tent", ViewValue = "plandeka", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "isotherm", ViewValue = "izoterma", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "box-truck", ViewValue = "kontener", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "spacious", ViewValue = "przestrzenne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "other", ViewValue = "inne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "car-transporter", ViewValue = "laweta", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "double-trailer", ViewValue = "zestaw", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "van", ViewValue = "van/bus", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "mega", ViewValue = "mega", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "coilmulde", ViewValue = "colimulde", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "walking-floor", ViewValue = "ruchoma podłoga", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "low-suspension", ViewValue = "niskopodwoziowe", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "flatbed", ViewValue = "platforma", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "chemical-tanker", ViewValue = "cysterna chemiczna", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "food-tanker", ViewValue = "cysterna spożywcza", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "petroleum-tanker", ViewValue = "cysterna paliwowa", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "gas-tanker", ViewValue = "cysterna gazowa", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "log-trailer", ViewValue = "dłużyca", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "oversized-cargo", ViewValue = "ponadgabaryt", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "hook-lift", ViewValue = "hakowiec", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "container-20-40", ViewValue = "kontener 20/40", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "dump-truck", ViewValue = "wywrotka", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "koffer", ViewValue = "koffer (stała zabudowa)", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "swap-body-system", ViewValue = "wymienne podwozie", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "jumbo", ViewValue = "jumbo", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "cooler", ViewValue = "chłodnia", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "curtainsider", ViewValue = "firanka", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "tanker", ViewValue = "cysterna", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "silos", ViewValue = "silos", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "removal-truck", ViewValue = "meblowóz", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "truckBody").FirstOrDefault() });

            ViewValueList.Add(new ViewValueDictionary { Value = "full-truck-standard", ViewValue = "całopojazdowy - firana Standard", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "full-truck-mega", ViewValue = "całopojazdowy - firana Mega", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "cubic", ViewValue = "objętościowy", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "bag", ViewValue = "worek", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "item", ViewValue = "sztuka towaru (bez specyfikacji)", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "envelope", ViewValue = "koperta", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "box", ViewValue = "skrzynia", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "package", ViewValue = "paczka", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "cardboard", ViewValue = "karton", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "timber", ViewValue = "dłużyca", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "roll", ViewValue = "rolka", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "roll2", ViewValue = "rulon", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "barrel", ViewValue = "beczka", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "other", ViewValue = "inne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "typeOfLoad").FirstOrDefault() });

            ViewValueList.Add(new ViewValueDictionary { Value = "euro", ViewValue = "EURO", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "loadRoutePalletType").FirstOrDefault() });
            ViewValueList.Add(new ViewValueDictionary { Value = "other", ViewValue = "Inne", ViewValueGroupName = dbGroupNames.Where(w => w.Name == "loadRoutePalletType").FirstOrDefault() });

            foreach (var view in ViewValueList)
            {
                if (dbViewList.Where(w => w.Value == view.Value).FirstOrDefault()==null)
                {
                    this._db.Entry(view).State = EntityState.Added;
                }
            }

            await this._db.SaveChangesAsync();
        }

    }
}
