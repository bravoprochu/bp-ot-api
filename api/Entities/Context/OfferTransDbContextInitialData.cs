using bp.ot.s.API.Entities.Dane.Invoice;
using bp.Pomocne.IdentityHelp.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Context
{

    public class OfferTransDbContextInitialDataIdent
    {
        private readonly OfferTransDbContextIdent _identContext;
        private readonly OfferTransDbContextDane _daneContext;

        public OfferTransDbContextInitialDataIdent(OfferTransDbContextIdent identContext, OfferTransDbContextDane daneContext)
        {
            _identContext = identContext;
            _daneContext = daneContext;
        }

        public async Task Initialize()
        {
            var roles = _identContext.Roles.ToList();

            await CreateRole(roles, IdentConst.Administrator);
            await CreateRole(roles, IdentConst.Manager);
            await CreateRole(roles, IdentConst.Spedytor);
            await CreateRole(roles, IdentConst.Finanse);
            if (_daneContext.Currency.Where(w => w.CurrencyId == 5).FirstOrDefault() == null)
            {
                this.CurrencyInit();

                await _daneContext.SaveChangesAsync();
            }

            if (_daneContext.PaymentTerm.Where(w => w.PaymentTermId == 2).FirstOrDefault() == null) {
                this.PaymentTermInit();
                await _daneContext.SaveChangesAsync();
            }

            AssignRoles(_identContext.Roles.Where(w => w.Name == IdentConst.Administrator).FirstOrDefault());
            await _identContext.SaveChangesAsync();

        }

        private async Task CreateRole(List<IdentityRole> roleBaseList, string roleName)
        {
            if (roleBaseList.Find(f => f.Name == roleName) == null)
            {
                await _identContext.Roles.AddAsync(new IdentityRole(roleName));
            }
        }

        private void AssignRoles(IdentityRole adminRole)
        {
            var bravoprochu = _identContext.Users.Where(w => w.UserName == "bravoprochu@gmail.com").Select(s => s).FirstOrDefault();
            if (bravoprochu != null) {return; }
            if (_identContext.UserRoles.Where(w => w.UserId == bravoprochu.Id && w.RoleId==adminRole.Id).FirstOrDefault() != null) { return; }
            _identContext.UserRoles.Add(new IdentityUserRole<string>()
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
                this._daneContext.Entry(curr).State = Microsoft.EntityFrameworkCore.EntityState.Added;
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
                this._daneContext.Entry(term).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }
        }

    }
}
