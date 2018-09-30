using bp.ot.s.API.Models.UserManagement;
using bp.shared.IdentityHelp.DTO;
using System.Collections.Generic;

namespace bp.ot.s.API.Models.UsersManagement
{
    public class UsersManagementDTO
    {
        public UsersManagementDTO()
        {
            this.Roles = new List<RoleDTO>();
        }

        public List<OtsUserDTO> Users { get; set; }
        public List<RoleDTO> Roles { get; set; }
    }
}
