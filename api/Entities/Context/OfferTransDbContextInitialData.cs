using System.Collections.Generic;
using System.Linq;

namespace bp.ot.s.API.Entities.Context
{
    public static class OfferTransDbContextInitialData
    {
        public static void Initialize(OfferTransDbContextDane db)
        {

        }
    }


    public static class OfferTransDbContextInitialDataIdent
    {
        public static void Initialize(OfferTransDbContextIdent db)
        {
            if (!db.Roles.Any()) {
                db.Roles.Add(new Microsoft.AspNetCore.Identity.IdentityRole
                {
                    Id = "Administrator",
                    Name = "Admin"
                });
                db.Roles.Add(new Microsoft.AspNetCore.Identity.IdentityRole
                {
                    Id="Spedytor",
                    Name="Spedytor"
                });
            }

            db.SaveChangesAsync();
        }
    }
}
