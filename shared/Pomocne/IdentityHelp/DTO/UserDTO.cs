using bp.Pomocne.DTO;
using bp.Pomocne.Pomocne.IdentityHelp.DTO;
using System.Collections.Generic;

namespace bp.Pomocne.IdentityHelp.DTO
{
    public class UserDTO:StatusDTO
    {
        public string UserId { get; set; }
        public string Nazwa { get; set; }
        public List<RoleDTO> Roles { get; set; }
    }
}
