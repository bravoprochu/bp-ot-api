using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bp.shared.IdentityHelp.DTO;

namespace bp.ot.s.API.Models.UserManagement
{
    public class OtsUserDTO: UserDTO
    {
        //public OtsUserDTO()
        //{
        //    this.Roles = new List<RoleDTO>();
        //}
        public string Email { get; set; }
        public string TransId { get; set; }
        public string TransUserSecret { get; set; }
        public new List<string> Roles { get; set; }
    }
}
