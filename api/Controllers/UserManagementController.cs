using api.Models;
using bp.ot.s.API.Entities.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bp.Pomocne.Linq;
using bp.ot.s.API.Models.UserManagement;
using bp.Pomocne.IdentityHelp.DTO;
using bp.Pomocne.ModelStateHelpful;


namespace bp.ot.s.API.Controllers
{
//    [Authorize]
    public class UsersManagementController: Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OfferTransDbContextIdent _identContext;

        public UsersManagementController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, OfferTransDbContextIdent contextIdent)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _identContext = contextIdent;
        }

        [HttpGet]
        public IActionResult GetUsersAndRoles() {

            var rolesBase = _roleManager.Roles.ToList();
            var userRoles = _identContext.UserRoles.ToList();
            var userRolesList = userRoles.GroupBy(g => g.UserId).Select(s => new {
                UserId=s.Key,
                Roles= s.Select(sr=> new RoleDTO()
                {
                    Nazwa = rolesBase.Where(w=>w.Id==sr.RoleId).FirstOrDefault().Name,
                    RoleId = sr.RoleId
                }).ToList()
            }).ToList();

            var usersList = _identContext.Users.Where(w => w.EmailConfirmed).Select(s => new OtsUserDTO()
            {
                TransId = s.TransId,
                TransUserSecret = s.TransUserSecret,
                UserId = s.Id,
                UserName = s.UserName,
                Email = s.Email
            }).ToList();

            foreach (var user in usersList)
            {
                user.Roles = userRolesList.Where(w => w.UserId == user.UserId).SelectMany(sm => sm.Roles).ToList();
            }

            return Ok(new {
                Users=usersList,
                Roles=rolesBase
            });
        }
        
        private async Task<IActionResult> UpdateUser(OtsUserDTO user) {
            var userDb = await _identContext.Users.FindAsync(user.UserId);
            if (userDb == null) {

            }
            return Ok();
        }

    }
}
