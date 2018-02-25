using bp.ot.s.API.Models.UserManagement;
using bp.shared.IdentityHelp.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
