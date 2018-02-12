using bp.Pomocne.DTO;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace bp.Pomocne
{
    public class CommonFunctions
    {
        public CreationInfo EtDTOCreationInfoMapper(CreationInfo db)
        {
            return new CreationInfo
            {
                CreatedBy = db.CreatedBy,
                CreatedDateTime = db.CreatedDateTime,
                ModifyBy = db.ModifyBy,
                ModifyDateTime = db.ModifyDateTime
            };
        }

        public void CreationInfoUpdate(CreationInfo db, CreationInfo dto, ClaimsPrincipal user)
        {
            string userName = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            dto = dto ?? new CreationInfo();

            if (string.IsNullOrEmpty(db.CreatedBy))
            {
                db.CreatedBy = userName;
                db.CreatedDateTime = DateTime.Now;
            }
            else
            {
                db.ModifyBy = userName;
                db.ModifyDateTime = DateTime.Now;
            }


        }
    }
}
