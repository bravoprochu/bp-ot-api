using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Entities.Context
{
    public static class DbInitIdent
    {
        public async static void Seed(RoleManager<IdentityRole> roleManager) {
            if (!await roleManager.RoleExistsAsync(IdentConst.Administrator)) {
                await roleManager.CreateAsync(new IdentityRole(IdentConst.Administrator));
            }
        }
    }
}
