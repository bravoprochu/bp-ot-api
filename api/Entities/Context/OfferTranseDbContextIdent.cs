using api.Models;
using bp.ot.s.API.Entities.Ident;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace bp.ot.s.API.Entities.Context
{
    public class OfferTransDbContextIdent : IdentityDbContext<ApplicationUser>
    {
        public OfferTransDbContextIdent(DbContextOptions<OfferTransDbContextIdent> options) : base(options)
        {
        }
    }
}
