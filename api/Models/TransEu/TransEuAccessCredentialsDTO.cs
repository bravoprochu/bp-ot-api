using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.TransEu
{
    public class TransEuAccessCredentialsDTO
    {
        public string UserName { get; set; }
        public string TransId { get; set; }
        public TransEuTokenDTO Token { get; set; }
        public DateTime ValidUntil { get {
                return this.Token.ValidUntil;
            }}
    }
}
