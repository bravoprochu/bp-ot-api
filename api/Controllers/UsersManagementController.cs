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
using bp.PomocneLocal.ModelStateHelpful;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using bp.ot.s.API.Models.UsersManagement;

namespace bp.ot.s.API.Controllers
{
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    [Route("api/[controller]/[action]")]
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



        public IActionResult GetAll()
        {
            return Ok(this.UsersManagementList());
        }



        [HttpPost]
        public async Task<IActionResult> Update([FromBody] List<OtsUserDTO> users)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelStateHelpful.ModelError());
            }
            List<string> result = new List<string>();

            foreach (var user in users)
            {
                if ((int)user.Status > 1)
                {
                    await UpdateUser(user, result);
                }
            }

            _identContext.SaveChanges();

            var data = this.UsersManagementList();

            return Ok(new {Info=result, Data= data });
        }



        private UsersManagementDTO UsersManagementList() {

            var rolesBase = _roleManager.Roles.ToList();
            var userRoles = _identContext.UserRoles.ToList();
            var userRolesList = userRoles.GroupBy(g => g.UserId).Select(s => new {
                UserId=s.Key,
                Roles= s.Select(sr=> new RoleDTO()
                {
                    Name = rolesBase.Where(w=>w.Id==sr.RoleId).FirstOrDefault().Name,
                    RoleId = sr.RoleId
                }).ToList()
            }).ToList();

            var usersList = _identContext.Users.Where(w => w.EmailConfirmed).Select(s => new OtsUserDTO()
            {
                TransId = s.TransId,
                TransUserSecret = s.TransUserSecret,
                UserId = s.Id,
                UserName = s.UserName,
                Email = s.Email,
                Status= Pomocne.Constansts.StatusEnum.Baza
            }).ToList();

            foreach (var user in usersList)
            {
                user.Roles = userRolesList.Where(w => w.UserId == user.UserId).SelectMany(sm => sm.Roles).ToList().Select(s=>s.RoleId).ToList();
            }

            return new UsersManagementDTO {
                Users = usersList,
                Roles = rolesBase.Select(s => new RoleDTO() {
                    Name = s.Name,
                    RoleId = s.Id
                }).ToList()
            };
        }


       
        private async Task UpdateUser(OtsUserDTO user, List<string> resultInfo) {
            var userDb = await _identContext.Users.FindAsync(user.UserId);
            if (userDb != null) {
                if (userDb.Email != user.Email)
                {
                    userDb.EmailConfirmed = false;
                    userDb.Email = user.Email;
                    resultInfo.Add("Adres email został zmieniony, należy go ponownie potwierdzić");
                }
                userDb.TransId = user.TransId;
                userDb.TransUserSecret = user.TransUserSecret;
                await UpdateRoles(userDb, user.Roles, resultInfo);
                _identContext.Entry(userDb).State = EntityState.Modified;
                
            }
            return;
        }

        private async Task UpdateRoles(ApplicationUser user, List<string> rolesIds, List<string> resultInfo)
        {
            var rolesDb = await _userManager.GetRolesAsync(user);
            var roles = _roleManager.Roles.ToList();

            foreach (var roleName in rolesDb)
            {
                var role = roles.Find(f => f.Name == roleName);
                if (!rolesIds.Any(a => a == role.Id))
                {
                    var userRole = _identContext.UserRoles.Where(w => w.RoleId == role.Id && w.UserId == user.Id).FirstOrDefault();
                    if (userRole != null) {
                        //                        _identContext.UserRoles.Remove(userRole);
                        _identContext.Entry(userRole).State = EntityState.Deleted;
                        resultInfo.Add($"Usunięto uprawnienia {role.Name} dla użytkownika {user.UserName}");
                    }
                }
                else {
                    rolesIds.Remove(role.Id);
                }
            }

            foreach (var roleId in rolesIds)
            {
                var role = roles.Find(f => f.Id == roleId);
                _identContext.UserRoles.Add(new IdentityUserRole<string>
                {
                    RoleId = role.Id,
                    UserId = user.Id
                });
                resultInfo.Add($"Dodano uprawnienia {role.Name} dla użytkownika {user.UserName}");
                //await _userManager.AddToRoleAsync(user, role.Name);
            }

            return;
        }

    }
}
