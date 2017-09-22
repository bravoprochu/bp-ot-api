using bp.Pomocne.IdentityHelp.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Context
{
    //public static class OfferTransDbContextInitialData
    //{
    //    public static void Initialize()
    //    {

    //    }
    //}


    public class OfferTransDbContextInitialDataIdent: IDbInitializer
    {
        private readonly OfferTransDbContextIdent _identContext;

        public OfferTransDbContextInitialDataIdent(OfferTransDbContextIdent identContext)
        {
            _identContext = identContext;
        }


        public async void Initialize()
        {
                var roles = _identContext.Roles.ToList();

                await CreateRole(roles, IdentConst.Administrator);
                await CreateRole(roles, IdentConst.Manager);
                await CreateRole(roles, IdentConst.Spedytor);

            await _identContext.SaveChangesAsync();
        }

        private async Task CreateRole(List<IdentityRole> roleBaseList, string roleName)
        {
            if (roleBaseList.Find(f => f.Name == roleName) == null)
            {
                await _identContext.Roles.AddAsync(new IdentityRole(roleName));
            }
        }
    }
}
