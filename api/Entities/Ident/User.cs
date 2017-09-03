using Microsoft.AspNetCore.Identity;

namespace bp.ot.s.API.Entities.Ident
{
    public class User: IdentityUser
    {
        public string TransId { get; set; }
    }
}
