using bp.shared.IdentityHelp.DTO;
using System.Collections.Generic;

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
