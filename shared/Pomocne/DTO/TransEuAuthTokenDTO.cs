using System;
using System.Collections.Generic;
using System.Text;

namespace bp.Pomocne.DTO
{
    public class TransEuAuthTokenDTO
    {
        public string Grant_type { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Client_id { get; set; }
        public string Client_secret { get; set; }
        public string Scope { get; set; }
    }
}
